using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Movies.Infrastructure.Tmdb;
using Testcontainers.PostgreSql;

namespace Movies.IntegrationTests.Infrastructure;

/// <summary>
/// Runs the real API against a real PostgreSQL (in Docker, via Testcontainers), seeded from a small
/// CSV of real rows (TestData/movies.csv), with cast data loaded from <see cref="FakeTmdbClient"/>.
/// The in-memory EF provider would not test what matters here: LIKE escaping, trigram indexes,
/// collations and the SQL behind paging.
/// </summary>
public sealed class MoviesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public FakeTmdbClient Tmdb { get; } = new();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        // Creating the server runs Program.cs: migrations, then the CSV import.
        _ = Server;

        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CastSyncer>().SyncAsync(CancellationToken.None);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Movies"] = _database.GetConnectionString(),
            ["Database:SeedCsvPath"] = Path.Combine(AppContext.BaseDirectory, "TestData", "movies.csv"),
            ["Tmdb:ApiToken"] = "", // the background sync stays off; InitializeAsync runs it once instead
        }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITmdbClient>();
            services.AddSingleton<ITmdbClient>(Tmdb);
        });
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<MoviesApiFactory>
{
    public const string Name = "API";
}
