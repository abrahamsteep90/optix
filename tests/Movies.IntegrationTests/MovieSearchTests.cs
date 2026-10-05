using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Movies.IntegrationTests.Infrastructure;

namespace Movies.IntegrationTests;

/// <summary>GET /api/movies against the 14 movies in TestData/movies.csv.</summary>
[Collection(ApiCollection.Name)]
public sealed class MovieSearchTests(MoviesApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Without_parameters_returns_the_first_page_most_popular_first()
    {
        var page = await _client.GetMoviesAsync();

        Assert.Equal(14, page.TotalCount);
        Assert.Equal(1, page.Page);
        Assert.Equal(20, page.PageSize);
        Assert.Equal(["Spider-Man: No Way Home", "The Batman", "Encanto"], page.Items.Take(3).Select(m => m.Title));
    }

    [Fact]
    public async Task Search_finds_part_of_a_title_ignoring_case() =>
        Assert.Equal(["The Batman", "Batman", "Batman Begins"], await _client.GetTitlesAsync("?search=BATMAN"));

    [Theory]
    [InlineData("pokemon", "Pokémon Detective Pikachu")]
    [InlineData("aeon", "Æon Flux")]
    public async Task Search_ignores_accents(string search, string expected) =>
        Assert.Equal([expected], await _client.GetTitlesAsync($"?search={search}"));

    [Theory]
    [InlineData("%25", "100% Wolf")]
    [InlineData("_", "Shiny_Flakes: The Teenage Drug Lord")]
    public async Task Search_treats_wildcard_characters_as_plain_text(string search, string expected) =>
        Assert.Equal([expected], await _client.GetTitlesAsync($"?search={search}"));

    [Fact]
    public async Task Page_size_limits_the_results_and_the_paging_numbers_add_up()
    {
        var json = await _client.GetFromJsonAsync<JsonElement>("/api/movies?pageSize=5&page=3");

        Assert.Equal(4, json.GetProperty("items").GetArrayLength()); // 14 movies = 5 + 5 + 4
        Assert.Equal(3, json.GetProperty("page").GetInt32());
        Assert.Equal(5, json.GetProperty("pageSize").GetInt32());
        Assert.Equal(14, json.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, json.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task A_page_past_the_end_is_empty()
    {
        var page = await _client.GetMoviesAsync("?page=99");

        Assert.Empty(page.Items);
        Assert.Equal(14, page.TotalCount);
    }

    [Fact]
    public async Task Paging_through_sorted_results_returns_every_movie_exactly_once()
    {
        // Two movies are called "Beauty and the Beast": without a tie-breaker one could appear on two pages.
        var ids = new List<int>();
        for (var page = 1; page <= 14; page++)
        {
            ids.AddRange((await _client.GetMoviesAsync($"?sortBy=title&pageSize=1&page={page}")).Items.Select(m => m.Id));
        }

        Assert.Equal(14, ids.Distinct().Count());
    }

    [Fact]
    public async Task Sorting_by_title_is_alphabetical_with_accented_letters_in_place() =>
        Assert.Equal(
            ["100% Wolf", "Æon Flux", "Batman", "Batman Begins", "Beauty and the Beast", "Beauty and the Beast"],
            await _client.GetTitlesAsync("?sortBy=title&pageSize=6"));

    [Fact]
    public async Task Sorting_by_release_date_shows_the_newest_first_unless_asked_otherwise()
    {
        Assert.Equal(
            ["Sonic the Hedgehog 2", "The Batman", "Spider-Man: No Way Home"],
            (await _client.GetTitlesAsync("?sortBy=releaseDate")).Take(3));
        Assert.Equal(
            ["Batman", "Beauty and the Beast", "Toy Story"],
            (await _client.GetTitlesAsync("?sortBy=releaseDate&sortDirection=asc")).Take(3));
    }

    [Theory]
    [InlineData("desc", "Spider-Man: No Way Home")]
    [InlineData("asc", "Æon Flux")]
    public async Task Sorting_by_rating_puts_movies_without_votes_last_either_way(string direction, string first)
    {
        var movies = (await _client.GetMoviesAsync($"?sortBy=rating&sortDirection={direction}")).Items;

        Assert.Equal(first, movies[0].Title);
        Assert.Equal("Sonic the Hedgehog 2", movies[^1].Title);
        Assert.Null(movies[^1].VoteAverage);
    }

    [Fact]
    public async Task Genre_filter_needs_every_genre_and_ignores_case()
    {
        Assert.Equal(5, (await _client.GetMoviesAsync("?genres=animation")).TotalCount);
        Assert.Equal(["Encanto", "Toy Story"], await _client.GetTitlesAsync("?genres=Animation&genres=FAMILY&genres=comedy"));
    }

    [Fact]
    public async Task Filters_can_be_combined() =>
        Assert.Equal(["Batman Begins"], await _client.GetTitlesAsync("?search=batman&genres=Drama"));

    [Theory]
    [InlineData("pageSize=0", "pageSize")]
    [InlineData("pageSize=101", "pageSize")]
    [InlineData("page=0", "page")]
    [InlineData("page=abc", "page")]
    [InlineData("sortBy=banana", "sortBy")]
    [InlineData("sortBy=99", "sortBy")]
    [InlineData("sortDirection=up", "sortDirection")]
    public async Task Invalid_parameters_get_a_400_that_names_the_problem(string query, string parameter)
    {
        using var response = await _client.GetAsync($"/api/movies?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(parameter, problem!.Errors.Keys);
    }
}
