using System.Globalization;
using Movies.Domain;

namespace Movies.Infrastructure.Tmdb;

/// <summary>What we know about one of our movies when looking for it on TMDB.</summary>
public sealed record MovieIdentity(string Title, DateOnly ReleaseDate, string PosterUrl);

/// <summary>
/// Decides which TMDB search result (if any) is the same movie as one in our CSV. The CSV has no TMDB ids,
/// so this is careful: a wrong match would show the wrong cast, which is worse than showing none.
/// </summary>
public static class TmdbMovieMatcher
{
    private const int MaxDaysApart = 366;

    public static TmdbMovie? FindMatch(MovieIdentity movie, IReadOnlyList<TmdbMovie> candidates)
    {
        // 1. Same poster file: the CSV's poster URLs point at TMDB's own images, so this is TMDB's record.
        var posterPath = "/" + Path.GetFileName(new Uri(movie.PosterUrl).AbsolutePath);
        var samePoster = candidates.FirstOrDefault(c => c.PosterPath == posterPath);
        if (samePoster is not null)
        {
            return samePoster;
        }

        var title = SearchText.Normalize(movie.Title);
        var dated = candidates
            .Select(c => (Movie: c, DaysApart: DaysApart(c.ReleaseDate, movie.ReleaseDate)))
            .Where(x => x.DaysApart <= MaxDaysApart)
            .OrderBy(x => x.DaysApart)
            .ToList();

        // 2. Same title, released within a year of our date (TMDB sometimes corrects release dates).
        var sameTitle = dated.FirstOrDefault(x => HasTitle(x.Movie, title)).Movie;
        if (sameTitle is not null)
        {
            return sameTitle;
        }

        // 3. Released on exactly the same day, with one title containing the other
        //    (e.g. a subtitle added or dropped since the dataset was made).
        return dated.FirstOrDefault(x => x.DaysApart == 0 && TitlesOverlap(x.Movie, title)).Movie;
    }

    private static bool HasTitle(TmdbMovie candidate, string title) =>
        SearchText.Normalize(candidate.Title) == title ||
        (candidate.OriginalTitle is not null && SearchText.Normalize(candidate.OriginalTitle) == title);

    private static bool TitlesOverlap(TmdbMovie candidate, string title)
    {
        var other = SearchText.Normalize(candidate.Title);
        return other.Length > 0 && (other.Contains(title) || title.Contains(other));
    }

    private static int DaysApart(string? tmdbDate, DateOnly ourDate) =>
        DateOnly.TryParseExact(tmdbDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? Math.Abs(date.DayNumber - ourDate.DayNumber)
            : int.MaxValue;
}
