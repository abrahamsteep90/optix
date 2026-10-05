using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Genres;
using Movies.IntegrationTests.Infrastructure;
using Npgsql;

namespace Movies.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class ErrorHandlingAndDocsTests(MoviesApiFactory factory)
{
    [Theory]
    [InlineData(false, HttpStatusCode.InternalServerError, "Something went wrong")]
    [InlineData(true, HttpStatusCode.ServiceUnavailable, "The database is unavailable")]
    public async Task Unexpected_errors_become_a_problem_without_internal_details(
        bool databaseError, HttpStatusCode expectedStatus, string expectedTitle)
    {
        Exception error = databaseError
            ? new NpgsqlException("Connection refused (secret-host:5432)")
            : new InvalidOperationException("Something internal (secret-host:5432)");

        await using var failingApi = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IGenreService>(_ => new FailingGenreService(error))));

        using var response = await failingApi.CreateClient().GetAsync("/api/genres");

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret-host", body);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(expectedTitle, problem!.Title);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Health_check_reports_healthy() =>
        Assert.Equal("Healthy", await factory.CreateClient().GetStringAsync("/health"));

    [Fact]
    public async Task OpenApi_document_and_swagger_ui_are_served()
    {
        var client = factory.CreateClient();

        var document = await client.GetStringAsync("/openapi/v1.json");
        Assert.Contains("\"/api/movies\"", document);
        Assert.Contains("\"pageSize\"", document);

        using var swagger = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
    }

    private sealed class FailingGenreService(Exception error) : IGenreService
    {
        public Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken) => throw error;
    }
}
