using System.Net.Http.Json;
using Movies.Application.Actors;
using Movies.Application.Common;
using Movies.Application.Genres;
using Movies.IntegrationTests.Infrastructure;

namespace Movies.IntegrationTests;

/// <summary>GET /api/genres, GET /api/actors and the actor filter on GET /api/movies.</summary>
[Collection(ApiCollection.Name)]
public sealed class GenreAndActorTests(MoviesApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Genres_are_listed_alphabetically_with_movie_counts()
    {
        var genres = (await _client.GetFromJsonAsync<GenreDto[]>("/api/genres"))!;

        Assert.Equal(genres.Select(g => g.Name).Order(StringComparer.Ordinal), genres.Select(g => g.Name));
        Assert.Equal(5, genres.Single(g => g.Name == "Animation").MovieCount);
        Assert.Equal(1, genres.Single(g => g.Name == "Documentary").MovieCount);
    }

    [Fact]
    public async Task Actor_filter_matches_part_of_a_name_ignoring_accents() =>
        Assert.Equal(["The Batman"], await _client.GetTitlesAsync("?actors=zoe")); // Zoë Kravitz

    [Fact]
    public async Task Every_actor_filter_must_match()
    {
        Assert.Equal(["Batman", "Batman Begins"], await _client.GetTitlesAsync("?actors=michael"));
        Assert.Equal(["Batman Begins"], await _client.GetTitlesAsync("?actors=michael&actors=bale"));
    }

    [Fact]
    public async Task Actors_can_be_searched_by_name()
    {
        var page = (await _client.GetFromJsonAsync<PagedResult<ActorDto>>("/api/actors?search=MICHAEL"))!;

        Assert.Equal(["Michael Caine", "Michael Keaton"], page.Items.Select(a => a.Name));
        Assert.All(page.Items, actor => Assert.Equal(1, actor.MovieCount));
    }
}
