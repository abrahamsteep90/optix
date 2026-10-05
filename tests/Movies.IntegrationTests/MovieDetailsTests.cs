using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Movies.Application.Movies;
using Movies.IntegrationTests.Infrastructure;

namespace Movies.IntegrationTests;

/// <summary>GET /api/movies/{id}.</summary>
[Collection(ApiCollection.Name)]
public sealed class MovieDetailsTests(MoviesApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_the_movie_with_its_genres_and_cast_in_billing_order()
    {
        var movie = await GetMovieAsync("no way home");

        Assert.Equal("Spider-Man: No Way Home", movie.Title);
        Assert.Equal(new DateOnly(2021, 12, 15), movie.ReleaseDate);
        Assert.Equal(8.3m, movie.VoteAverage);
        Assert.Equal(["Action", "Adventure", "Science Fiction"], movie.Genres);
        Assert.Equal(["Tom Holland", "Zendaya", "Benedict Cumberbatch"], movie.Cast.Select(c => c.Name));
        Assert.Equal("Peter Parker / Spider-Man", movie.Cast[0].Character);
        Assert.Equal("https://image.tmdb.org/t/p/w185/tom-holland.jpg", movie.Cast[0].ProfileImageUrl);
        Assert.Null(movie.Cast[2].ProfileImageUrl);
    }

    [Fact]
    public async Task Unknown_id_gets_a_404_problem()
    {
        using var response = await _client.GetAsync("/api/movies/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(404, problem!.Status);
    }

    [Fact]
    public async Task The_description_from_the_broken_csv_row_keeps_its_line_breaks()
    {
        var movie = await GetMovieAsync("pixie hollow");

        Assert.Contains("Mini-Shorts:\n - Just Desserts\n - If The Hue Fits", movie.Overview);
    }

    [Fact]
    public async Task A_movie_nobody_voted_on_has_no_rating()
    {
        var movie = await GetMovieAsync("sonic");

        Assert.Equal(0, movie.VoteCount);
        Assert.Null(movie.VoteAverage);
    }

    private async Task<MovieDetailsDto> GetMovieAsync(string search)
    {
        var id = (await _client.GetMoviesAsync($"?search={Uri.EscapeDataString(search)}")).Items.Single().Id;
        return (await _client.GetFromJsonAsync<MovieDetailsDto>($"/api/movies/{id}"))!;
    }
}
