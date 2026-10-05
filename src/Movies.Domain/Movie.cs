namespace Movies.Domain;

public class Movie
{
    private string _title = "";

    public int Id { get; set; }

    public required string Title
    {
        get => _title;
        set
        {
            _title = value;
            SearchTitle = SearchText.Normalize(value);
        }
    }

    /// <summary>Lower-case, accent-free copy of <see cref="Title"/>, kept in sync automatically. Used for searching.</summary>
    public string SearchTitle { get; private set; } = "";

    public required string Overview { get; set; }

    public DateOnly ReleaseDate { get; set; }

    /// <summary>TMDB popularity score at the time the dataset was made (March 2022).</summary>
    public double Popularity { get; set; }

    public int VoteCount { get; set; }

    /// <summary>Average vote out of 10. Meaningless when <see cref="VoteCount"/> is 0.</summary>
    public decimal VoteAverage { get; set; }

    /// <summary>ISO 639-1 style code as TMDB uses it (e.g. "en", "ja"; TMDB uses "cn" for Cantonese).</summary>
    public required string OriginalLanguage { get; set; }

    public required string PosterUrl { get; set; }

    /// <summary>The movie's TMDB id, set when cast data has been fetched from TMDB.</summary>
    public int? TmdbId { get; set; }

    /// <summary>When cast data was last fetched from TMDB; null if never.</summary>
    public DateTimeOffset? CastSyncedAt { get; set; }

    public List<Genre> Genres { get; set; } = [];

    public List<CastMember> Cast { get; set; } = [];
}
