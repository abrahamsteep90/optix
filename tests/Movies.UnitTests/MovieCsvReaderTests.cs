using System.Globalization;
using Movies.Infrastructure.Seeding;

namespace Movies.UnitTests;

public class MovieCsvReaderTests
{
    private const string Header =
        "Release_Date,Title,Overview,Popularity,Vote_Count,Vote_Average,Original_Language,Genre,Poster_Url\n";

    private static readonly string DatasetPath = Path.Combine(AppContext.BaseDirectory, "TestData", "mymoviedb.csv");

    [Fact]
    public void Reads_every_movie_in_the_real_dataset()
    {
        var movies = MovieCsvReader.ReadFile(DatasetPath);

        // A default CSV reader finds 9,837 rows: one broken movie becomes 11 rows.
        Assert.Equal(9827, movies.Count);
        Assert.Equal(19, movies.SelectMany(m => m.Genres).Distinct().Count());
    }

    [Fact]
    public void Keeps_the_movie_with_unquoted_line_breaks_in_one_piece()
    {
        var movie = Assert.Single(MovieCsvReader.ReadFile(DatasetPath), m => m.Title == "Pixie Hollow Bake Off");

        Assert.Equal(new DateOnly(2013, 10, 20), movie.ReleaseDate);
        Assert.Equal(61.328, movie.Popularity);
        Assert.Equal(35, movie.VoteCount);
        Assert.Equal(7.1m, movie.VoteAverage);
        Assert.Equal("en", movie.OriginalLanguage);
        Assert.Equal(["Animation"], movie.Genres);
        Assert.StartsWith("Tink challenges Gelata", movie.Overview);
        Assert.Contains("Mini-Shorts:\n - Just Desserts\n - If The Hue Fits", movie.Overview);
        Assert.DoesNotContain('\r', movie.Overview);
    }

    [Fact]
    public void Reads_every_column()
    {
        var movie = MovieCsvReader.ReadFile(DatasetPath)[0];

        Assert.Equal(new DateOnly(2021, 12, 15), movie.ReleaseDate);
        Assert.Equal("Spider-Man: No Way Home", movie.Title);
        Assert.StartsWith("Peter Parker is unmasked", movie.Overview);
        Assert.Equal(5083.954, movie.Popularity);
        Assert.Equal(8940, movie.VoteCount);
        Assert.Equal(8.3m, movie.VoteAverage);
        Assert.Equal("en", movie.OriginalLanguage);
        Assert.Equal(["Action", "Adventure", "Science Fiction"], movie.Genres);
        Assert.Equal("https://image.tmdb.org/t/p/original/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg", movie.PosterUrl);
    }

    [Fact]
    public void Reads_numbers_the_same_whatever_the_current_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            // In Turkish (and German) "8.3" would mean 83, because the decimal separator is a comma.
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            var movie = Assert.Single(MovieCsvReader.Read(new StringReader(
                Header + "2021-12-15,Test,Text,5083.954,8940,8.3,en,Drama,https://image.tmdb.org/t/p/original/a.jpg\n")));

            Assert.Equal(5083.954, movie.Popularity);
            Assert.Equal(8.3m, movie.VoteAverage);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Survives_windows_line_endings()
    {
        // e.g. after a Git checkout on Windows with autocrlf turned on.
        var movie = Assert.Single(MovieCsvReader.Read(new StringReader(
            Header.Replace("\n", "\r\n") + "2021-12-15,Test,Text,1.5,10,7.0,en,Drama,https://x.test/a.jpg\r\n")));

        Assert.Equal("https://x.test/a.jpg", movie.PosterUrl);
    }

    [Fact]
    public void Says_which_line_is_broken()
    {
        var csv = Header +
            "2021-12-15,Good,Text,1.5,10,7.0,en,Drama,https://x.test/a.jpg\n" +
            "not-a-date,Bad,Text,1.5,10,7.0,en,Drama,https://x.test/b.jpg\n";

        var error = Assert.Throws<InvalidDataException>(() => MovieCsvReader.Read(new StringReader(csv)));

        Assert.StartsWith("Line 3 ", error.Message);
    }
}
