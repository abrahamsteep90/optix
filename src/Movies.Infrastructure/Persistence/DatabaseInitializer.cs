using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Movies.Domain;
using Movies.Infrastructure.Seeding;

namespace Movies.Infrastructure.Persistence;

/// <summary>
/// Brings the database up to date when the app starts, so "docker compose up" gives a ready-to-use API.
/// In a real deployment, migrations would run as a separate release step instead.
/// </summary>
public sealed class DatabaseInitializer(
    MoviesDbContext db,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Applying database migrations");

        // On a brand-new database EF Core reads its history table before creating it and logs the
        // (handled) failure as an error, which looks alarming on a first "docker compose up".
        await db.GetService<IHistoryRepository>().CreateIfNotExistsAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Movies.AnyAsync(cancellationToken))
        {
            logger.LogInformation("The database already has movies, so the CSV import is skipped");
            return;
        }

        var path = Path.GetFullPath(options.Value.SeedCsvPath, AppContext.BaseDirectory);
        logger.LogInformation("Importing movies from {Path}", path);

        var records = MovieCsvReader.ReadFile(path);
        var genres = records
            .SelectMany(r => r.Genres)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(name => name, name => new Genre { Name = name }, StringComparer.OrdinalIgnoreCase);

        db.Movies.AddRange(records.Select(r => new Movie
        {
            Title = r.Title,
            Overview = r.Overview,
            ReleaseDate = r.ReleaseDate,
            Popularity = r.Popularity,
            VoteCount = r.VoteCount,
            VoteAverage = r.VoteAverage,
            OriginalLanguage = r.OriginalLanguage,
            PosterUrl = r.PosterUrl,
            Genres = r.Genres.Select(name => genres[name]).ToList(),
        }));

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Imported {MovieCount} movies in {GenreCount} genres", records.Count, genres.Count);
    }
}
