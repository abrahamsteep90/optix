using System.ComponentModel.DataAnnotations;

namespace Movies.Infrastructure.Tmdb;

public sealed class TmdbOptions
{
    public const string SectionName = "Tmdb";

    /// <summary>
    /// TMDB "API Read Access Token" (the long one, from themoviedb.org/settings/api).
    /// Cast data is only fetched when this is set.
    /// </summary>
    public string? ApiToken { get; set; }

    [Required]
    public Uri BaseUrl { get; set; } = new("https://api.themoviedb.org/3/");

    /// <summary>How many top-billed actors to keep for each movie.</summary>
    [Range(1, 50)]
    public int CastPerMovie { get; set; } = 10;

    /// <summary>TMDB allows roughly 40 requests a second; staying well below that keeps us polite.</summary>
    [Range(1, 40)]
    public int RequestsPerSecond { get; set; } = 20;

    /// <summary>
    /// TMDB's terms of use don't allow keeping their data for more than six months,
    /// so cast data older than this is fetched again.
    /// </summary>
    [Range(1, 180)]
    public int RefreshAfterDays { get; set; } = 150;
}
