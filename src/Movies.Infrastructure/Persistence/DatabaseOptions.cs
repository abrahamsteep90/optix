namespace Movies.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply migrations and, if the database is empty, import the CSV when the app starts.</summary>
    public bool InitializeOnStartup { get; set; } = true;

    /// <summary>The movies CSV, either absolute or relative to the app's folder.</summary>
    public string SeedCsvPath { get; set; } = Path.Combine("Data", "mymoviedb.csv");
}
