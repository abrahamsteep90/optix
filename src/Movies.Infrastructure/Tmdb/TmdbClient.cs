using System.Net.Http.Json;
using System.Text.Json;

namespace Movies.Infrastructure.Tmdb;

/// <summary>A movie in TMDB search results.</summary>
/// <param name="ReleaseDate">"yyyy-MM-dd", or empty when TMDB doesn't know it.</param>
/// <param name="PosterPath">e.g. "/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg".</param>
public sealed record TmdbMovie(int Id, string Title, string? OriginalTitle, string? ReleaseDate, string? PosterPath);

/// <summary>One cast credit of a movie on TMDB.</summary>
/// <param name="Id">The person's TMDB id.</param>
/// <param name="Order">Billing order (0 = top billed).</param>
public sealed record TmdbCastCredit(int Id, string Name, string? Character, int Order, string? ProfilePath);

public interface ITmdbClient
{
    /// <param name="year">Only movies first released in this year, or null for any year.</param>
    Task<IReadOnlyList<TmdbMovie>> SearchMoviesAsync(string title, int? year, CancellationToken cancellationToken);

    Task<IReadOnlyList<TmdbCastCredit>> GetCastAsync(int tmdbMovieId, CancellationToken cancellationToken);
}

/// <summary>
/// Calls the TMDB API (https://developer.themoviedb.org). The base address, token, retries and
/// rate limiting are set up in <see cref="DependencyInjection"/>.
/// </summary>
internal sealed class TmdbClient(HttpClient http) : ITmdbClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task<IReadOnlyList<TmdbMovie>> SearchMoviesAsync(
        string title, int? year, CancellationToken cancellationToken)
    {
        var url = $"search/movie?query={Uri.EscapeDataString(title)}&include_adult=false&language=en-US";
        if (year is not null)
        {
            url += $"&primary_release_year={year}";
        }

        var response = await http.GetFromJsonAsync<SearchResponse>(url, Json, cancellationToken);
        return response?.Results ?? [];
    }

    public async Task<IReadOnlyList<TmdbCastCredit>> GetCastAsync(int tmdbMovieId, CancellationToken cancellationToken)
    {
        var response = await http.GetFromJsonAsync<CreditsResponse>(
            $"movie/{tmdbMovieId}/credits?language=en-US", Json, cancellationToken);
        return response?.Cast ?? [];
    }

    private sealed record SearchResponse(IReadOnlyList<TmdbMovie>? Results);

    private sealed record CreditsResponse(IReadOnlyList<TmdbCastCredit>? Cast);
}
