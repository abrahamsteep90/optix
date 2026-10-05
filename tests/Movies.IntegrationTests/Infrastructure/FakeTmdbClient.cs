using Movies.Infrastructure.Tmdb;

namespace Movies.IntegrationTests.Infrastructure;

/// <summary>
/// Stands in for TMDB with a few hand-picked movies, so the cast sync runs end to end without a token.
/// Each movie is set up to exercise one of the matching rules.
/// </summary>
public sealed class FakeTmdbClient : ITmdbClient
{
    private readonly List<FakeMovie> _movies =
    [
        // Matched by poster: the poster path is the one in the CSV.
        new(new TmdbMovie(634649, "Spider-Man: No Way Home", "Spider-Man: No Way Home", "2021-12-15", "/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg"),
        [
            new(1136406, "Tom Holland", "Peter Parker / Spider-Man", 0, "/tom-holland.jpg"),
            new(505710, "Zendaya", "MJ", 1, "/zendaya.jpg"),
            new(71580, "Benedict Cumberbatch", "Stephen Strange", 2, null),
        ]),

        // Matched by title and release date (TMDB has a different poster now).
        new(new TmdbMovie(414906, "The Batman", "The Batman", "2022-03-01", "/a-newer-poster.jpg"),
        [
            new(11288, "Robert Pattinson", "Bruce Wayne / The Batman", 0, null),
            new(37917, "Zoë Kravitz", "Selina Kyle / Catwoman", 1, null),
        ]),

        new(new TmdbMovie(268, "Batman", "Batman", "1989-06-23", "/batman-1989.jpg"),
        [
            new(2232, "Michael Keaton", "Batman / Bruce Wayne", 0, null),
            new(514, "Jack Nicholson", "Joker / Jack Napier", 1, null),
        ]),

        new(new TmdbMovie(272, "Batman Begins", "Batman Begins", "2005-06-10", "/batman-begins.jpg"),
        [
            new(3894, "Christian Bale", "Bruce Wayne / Batman", 0, null),
            new(3895, "Michael Caine", "Alfred Pennyworth", 1, null),
        ]),

        // Only found by the second search, without the year.
        new(new TmdbMovie(568124, "Encanto", "Encanto", "2021-11-24", "/encanto.jpg"),
        [
            new(1281250, "Stephanie Beatriz", "Mirabel Madrigal (voice)", 0, null),
        ],
        OnlyFoundWithoutYear: true),

        // The same person is credited twice.
        new(new TmdbMovie(447404, "Pokémon Detective Pikachu", "Pokémon Detective Pikachu", "2019-05-03", "/pikachu.jpg"),
        [
            new(10859, "Ryan Reynolds", "Detective Pikachu (voice)", 0, null),
            new(1003, "Justice Smith", "Tim Goodman", 1, null),
            new(10859, "Ryan Reynolds", "Harry Goodman", 5, null),
        ]),
    ];

    private int _searches;

    public int Searches => _searches;

    public Task<IReadOnlyList<TmdbMovie>> SearchMoviesAsync(string title, int? year, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _searches);

        IReadOnlyList<TmdbMovie> results = _movies
            .Where(m => m.Movie.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
            .Where(m => year is null || (!m.OnlyFoundWithoutYear && m.Movie.ReleaseDate!.StartsWith($"{year}-")))
            .Select(m => m.Movie)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IReadOnlyList<TmdbCastCredit>> GetCastAsync(int tmdbMovieId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TmdbCastCredit>>(_movies.Single(m => m.Movie.Id == tmdbMovieId).Cast);

    private sealed record FakeMovie(TmdbMovie Movie, TmdbCastCredit[] Cast, bool OnlyFoundWithoutYear = false);
}
