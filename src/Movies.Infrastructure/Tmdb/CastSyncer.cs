using System.Collections.Concurrent;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Movies.Domain;
using Movies.Infrastructure.Persistence;

namespace Movies.Infrastructure.Tmdb;

/// <summary>Thrown when TMDB rejects the token, so there is no point trying more movies.</summary>
public sealed class TmdbAuthenticationException(Exception inner)
    : Exception("TMDB rejected the API token.", inner);

/// <param name="Processed">Movies looked up on TMDB in this run.</param>
/// <param name="Matched">Movies found on TMDB (their cast is now stored).</param>
/// <param name="Failed">Movies skipped because of an error; they are tried again next run.</param>
public sealed record CastSyncSummary(int Processed, int Matched, int Failed);

/// <summary>
/// Fetches cast lists from TMDB for movies that have none yet, or whose cast is older than
/// <see cref="TmdbOptions.RefreshAfterDays"/>. Most popular movies go first.
/// </summary>
public sealed class CastSyncer(
    IServiceScopeFactory scopeFactory,
    ITmdbClient tmdb,
    IOptions<TmdbOptions> options,
    TimeProvider clock,
    ILogger<CastSyncer> logger)
{
    private const int BatchSize = 50;
    private const int ParallelRequests = 4;

    public async Task<CastSyncSummary> SyncAsync(CancellationToken cancellationToken)
    {
        var due = await GetMoviesDueAsync(cancellationToken);
        if (due.Count == 0)
        {
            return new CastSyncSummary(0, 0, 0);
        }

        logger.LogInformation("Fetching cast from TMDB for {Count} movies", due.Count);

        int processed = 0, matched = 0, failed = 0;
        foreach (var batch in due.Chunk(BatchSize))
        {
            var results = new ConcurrentBag<CastLookup>();
            await Parallel.ForEachAsync(
                batch,
                new ParallelOptions { MaxDegreeOfParallelism = ParallelRequests, CancellationToken = cancellationToken },
                async (movie, ct) =>
                {
                    if (await LookUpAsync(movie, ct) is { } lookup)
                    {
                        results.Add(lookup);
                    }
                });

            await SaveAsync(results, cancellationToken);

            processed += batch.Length;
            matched += results.Count(r => r.TmdbId is not null);
            failed += batch.Length - results.Count;
            logger.LogInformation(
                "Cast sync: {Processed} of {Total} movies done, {Matched} found on TMDB, {Failed} failed",
                processed, due.Count, matched, failed);
        }

        return new CastSyncSummary(processed, matched, failed);
    }

    private async Task<List<MovieToSync>> GetMoviesDueAsync(CancellationToken cancellationToken)
    {
        var staleBefore = clock.GetUtcNow().AddDays(-options.Value.RefreshAfterDays);

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();

        return await db.Movies
            .AsNoTracking()
            .Where(m => m.CastSyncedAt == null || m.CastSyncedAt < staleBefore)
            .OrderByDescending(m => m.Popularity)
            .ThenBy(m => m.Id)
            .Select(m => new MovieToSync(m.Id, new MovieIdentity(m.Title, m.ReleaseDate, m.PosterUrl)))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Finds the movie on TMDB and gets its cast. Returns null if TMDB could not be reached.</summary>
    private async Task<CastLookup?> LookUpAsync(MovieToSync movie, CancellationToken cancellationToken)
    {
        try
        {
            var identity = movie.Identity;
            var match =
                TmdbMovieMatcher.FindMatch(identity, await tmdb.SearchMoviesAsync(identity.Title, identity.ReleaseDate.Year, cancellationToken))
                ?? TmdbMovieMatcher.FindMatch(identity, await tmdb.SearchMoviesAsync(identity.Title, null, cancellationToken));

            if (match is null)
            {
                return new CastLookup(movie.Id, TmdbId: null, Cast: []);
            }

            var cast = await tmdb.GetCastAsync(match.Id, cancellationToken);
            return new CastLookup(movie.Id, match.Id, cast.OrderBy(c => c.Order).Take(options.Value.CastPerMovie).ToList());
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new TmdbAuthenticationException(ex);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Could not get cast for movie {MovieId} ({Title}); it will be retried on the next run",
                movie.Id, movie.Identity.Title);
            return null;
        }
    }

    private async Task SaveAsync(IReadOnlyCollection<CastLookup> lookups, CancellationToken cancellationToken)
    {
        if (lookups.Count == 0)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
        var now = clock.GetUtcNow();

        // The execution strategy retries the whole unit if the connection drops, so everything inside starts clean.
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var movieIds = lookups.Select(l => l.MovieId).ToList();
            await db.CastMembers.Where(c => movieIds.Contains(c.MovieId)).ExecuteDeleteAsync(cancellationToken);

            var personIds = lookups.SelectMany(l => l.Cast).Select(c => c.Id).Distinct().ToList();
            var actors = await db.Actors
                .Where(a => personIds.Contains(a.TmdbPersonId))
                .ToDictionaryAsync(a => a.TmdbPersonId, cancellationToken);
            var movies = await db.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            foreach (var lookup in lookups)
            {
                var movie = movies[lookup.MovieId];
                movie.TmdbId = lookup.TmdbId;
                movie.CastSyncedAt = now;

                // TMDB can credit one person twice in a movie (two characters): keep one role with both names.
                foreach (var credits in lookup.Cast.GroupBy(c => c.Id))
                {
                    var credit = credits.First();
                    if (!actors.TryGetValue(credit.Id, out var actor))
                    {
                        actor = new Actor { TmdbPersonId = credit.Id, Name = credit.Name };
                        actors.Add(credit.Id, actor);
                        db.Actors.Add(actor);
                    }

                    actor.Name = credit.Name;
                    actor.ProfilePath = credit.ProfilePath;

                    db.CastMembers.Add(new CastMember
                    {
                        Movie = movie,
                        Actor = actor,
                        Character = JoinCharacters(credits),
                        BillingOrder = credit.Order,
                    });
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static string? JoinCharacters(IEnumerable<TmdbCastCredit> credits)
    {
        var characters = credits.Select(c => c.Character).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
        return characters.Count == 0 ? null : string.Join(" / ", characters);
    }

    private sealed record MovieToSync(int Id, MovieIdentity Identity);

    private sealed record CastLookup(int MovieId, int? TmdbId, IReadOnlyList<TmdbCastCredit> Cast);
}
