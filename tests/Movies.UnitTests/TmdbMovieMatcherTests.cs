using Movies.Infrastructure.Tmdb;

namespace Movies.UnitTests;

public class TmdbMovieMatcherTests
{
    private static readonly MovieIdentity SpiderMan = new(
        "Spider-Man: No Way Home",
        new DateOnly(2021, 12, 15),
        "https://image.tmdb.org/t/p/original/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg");

    [Fact]
    public void Same_poster_file_is_a_match_even_if_title_and_date_changed()
    {
        var candidate = new TmdbMovie(634649, "Spider-Man 3: No Way Home", null, "2021-12-17", "/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg");

        Assert.Same(candidate, TmdbMovieMatcher.FindMatch(SpiderMan, [Other(1), candidate]));
    }

    [Fact]
    public void Same_title_and_release_date_is_a_match()
    {
        var candidate = new TmdbMovie(634649, "Spider-Man: No Way Home", null, "2021-12-15", "/new-poster.jpg");

        Assert.Same(candidate, TmdbMovieMatcher.FindMatch(SpiderMan, [Other(1), candidate]));
    }

    [Fact]
    public void Title_comparison_ignores_case_and_accents()
    {
        var ours = new MovieIdentity("Pokémon Detective Pikachu", new DateOnly(2019, 5, 3), "https://image.tmdb.org/t/p/original/a.jpg");
        var candidate = new TmdbMovie(447404, "POKEMON Detective Pikachu", null, "2019-05-03", "/b.jpg");

        Assert.Same(candidate, TmdbMovieMatcher.FindMatch(ours, [candidate]));
    }

    [Fact]
    public void Original_title_also_counts()
    {
        var ours = new MovieIdentity("Spirited Away", new DateOnly(2001, 7, 20), "https://image.tmdb.org/t/p/original/a.jpg");
        var candidate = new TmdbMovie(129, "Sen to Chihiro no Kamikakushi", "Spirited Away", "2001-07-20", "/b.jpg");

        Assert.Same(candidate, TmdbMovieMatcher.FindMatch(ours, [candidate]));
    }

    [Fact]
    public void Release_date_can_be_up_to_a_year_out_and_the_closest_wins()
    {
        var further = new TmdbMovie(1, SpiderMan.Title, null, "2022-09-01", "/a.jpg");
        var closer = new TmdbMovie(2, SpiderMan.Title, null, "2021-12-17", "/b.jpg");

        Assert.Same(closer, TmdbMovieMatcher.FindMatch(SpiderMan, [further, closer]));
    }

    [Fact]
    public void Remake_from_another_decade_is_not_a_match()
    {
        var ours = new MovieIdentity("Beauty and the Beast", new DateOnly(2017, 3, 16), "https://image.tmdb.org/t/p/original/a.jpg");
        var cartoon = new TmdbMovie(10020, "Beauty and the Beast", null, "1991-10-22", "/b.jpg");

        Assert.Null(TmdbMovieMatcher.FindMatch(ours, [cartoon]));
    }

    [Fact]
    public void Changed_subtitle_is_a_match_on_the_exact_release_date()
    {
        var candidate = new TmdbMovie(634649, "Spider-Man: No Way Home - Extended", null, "2021-12-15", "/b.jpg");

        Assert.Same(candidate, TmdbMovieMatcher.FindMatch(SpiderMan, [candidate]));
    }

    [Fact]
    public void Different_movie_on_the_same_day_is_not_a_match()
    {
        var sameDay = new TmdbMovie(5, "Nightmare Alley", null, "2021-12-15", "/b.jpg");

        Assert.Null(TmdbMovieMatcher.FindMatch(SpiderMan, [sameDay]));
    }

    [Fact]
    public void No_guess_without_a_release_date_or_without_results()
    {
        var undated = new TmdbMovie(634649, SpiderMan.Title, null, "", "/b.jpg");

        Assert.Null(TmdbMovieMatcher.FindMatch(SpiderMan, [undated]));
        Assert.Null(TmdbMovieMatcher.FindMatch(SpiderMan, []));
    }

    private static TmdbMovie Other(int id) => new(id, "Something Else", null, "1999-01-01", "/other.jpg");
}
