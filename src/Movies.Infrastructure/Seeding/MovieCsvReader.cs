using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Movies.Infrastructure.Seeding;

/// <summary>One row of the Kaggle "9000+ Movies" CSV.</summary>
public sealed record MovieCsvRecord(
    DateOnly ReleaseDate,
    string Title,
    string Overview,
    double Popularity,
    int VoteCount,
    decimal VoteAverage,
    string OriginalLanguage,
    IReadOnlyList<string> Genres,
    string PosterUrl);

public static class MovieCsvReader
{
    public static IReadOnlyList<MovieCsvRecord> ReadFile(string path)
    {
        using var reader = new StreamReader(path);
        return Read(reader);
    }

    public static IReadOnlyList<MovieCsvRecord> Read(TextReader reader)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // The description of "Pixie Hollow Bake Off" contains bare carriage returns ("\r") outside
            // quotes. By default a "\r" also ends a row, which splits that movie into 11 broken rows.
            // In this file only "\n" ends a row.
            NewLine = "\n",
            // With Windows line endings the last header would be "Poster_Url\r".
            PrepareHeaderForMatch = args => args.Header.Trim(),
        };

        using var csv = new CsvReader(reader, config);
        csv.Read();
        csv.ReadHeader();

        var records = new List<MovieCsvRecord>();
        while (csv.Read())
        {
            try
            {
                records.Add(ReadRecord(csv));
            }
            catch (Exception ex) when (ex is CsvHelperException or FormatException)
            {
                throw new InvalidDataException(
                    $"Line {csv.Parser.RawRow} of the movies CSV could not be read: {ex.Message}", ex);
            }
        }

        return records;
    }

    private static MovieCsvRecord ReadRecord(CsvReader csv) => new(
        ReleaseDate: DateOnly.ParseExact(Text(csv, "Release_Date"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
        Title: Text(csv, "Title"),
        Overview: Text(csv, "Overview"),
        Popularity: double.Parse(Text(csv, "Popularity"), CultureInfo.InvariantCulture),
        VoteCount: int.Parse(Text(csv, "Vote_Count"), CultureInfo.InvariantCulture),
        VoteAverage: decimal.Parse(Text(csv, "Vote_Average"), CultureInfo.InvariantCulture),
        OriginalLanguage: Text(csv, "Original_Language"),
        Genres: Text(csv, "Genre").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        PosterUrl: Text(csv, "Poster_Url"));

    /// <summary>A trimmed field, with any carriage returns turned into normal line breaks.</summary>
    private static string Text(CsvReader csv, string column) =>
        (csv.GetField(column) ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Trim();
}
