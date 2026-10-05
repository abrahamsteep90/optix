using System.Net.Http.Json;
using Movies.Application.Common;
using Movies.Application.Movies;

namespace Movies.IntegrationTests.Infrastructure;

internal static class HttpClientExtensions
{
    public static async Task<PagedResult<MovieSummaryDto>> GetMoviesAsync(this HttpClient client, string query = "") =>
        (await client.GetFromJsonAsync<PagedResult<MovieSummaryDto>>($"/api/movies{query}"))!;

    public static async Task<string[]> GetTitlesAsync(this HttpClient client, string query = "") =>
        (await client.GetMoviesAsync(query)).Items.Select(m => m.Title).ToArray();
}
