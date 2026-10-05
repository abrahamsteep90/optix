using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Movies;
using Movies.Infrastructure.Persistence;
using Movies.Infrastructure.Tmdb;
using Movies.IntegrationTests.Infrastructure;

namespace Movies.IntegrationTests;

/// <summary>
/// The TMDB cast sync with a fake TMDB. <see cref="MoviesApiFactory"/> already ran it once on startup.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class CastSyncTests(MoviesApiFactory factory)
{
    [Fact]
    public async Task Matched_movies_get_their_tmdb_id_and_every_movie_is_marked_as_checked()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var movies = await scope.ServiceProvider.GetRequiredService<MoviesDbContext>().Movies
            .AsNoTracking()
            .ToDictionaryAsync(m => $"{m.Title} ({m.ReleaseDate.Year})");

        Assert.Equal(634649, movies["Spider-Man: No Way Home (2021)"].TmdbId); // same poster
        Assert.Equal(414906, movies["The Batman (2022)"].TmdbId); // same title and date
        Assert.Equal(568124, movies["Encanto (2021)"].TmdbId); // found by the search without a year
        Assert.Null(movies["100% Wolf (2020)"].TmdbId); // not on (fake) TMDB
        Assert.All(movies.Values, movie => Assert.NotNull(movie.CastSyncedAt));
    }

    [Fact]
    public async Task A_person_credited_twice_gets_one_role_with_both_characters()
    {
        var client = factory.CreateClient();
        var id = (await client.GetMoviesAsync("?search=pikachu")).Items.Single().Id;

        var movie = (await client.GetFromJsonAsync<MovieDetailsDto>($"/api/movies/{id}"))!;

        Assert.Equal(["Ryan Reynolds", "Justice Smith"], movie.Cast.Select(c => c.Name));
        Assert.Equal("Detective Pikachu (voice) / Harry Goodman", movie.Cast[0].Character);
    }

    [Fact]
    public async Task Running_again_soon_after_has_nothing_to_do()
    {
        await using var scope = factory.Services.CreateAsyncScope();

        var summary = await scope.ServiceProvider.GetRequiredService<CastSyncer>().SyncAsync(CancellationToken.None);

        Assert.Equal(new CastSyncSummary(0, 0, 0), summary);
    }

    [Fact]
    public async Task Cast_older_than_the_refresh_period_is_fetched_again()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var syncer = ActivatorUtilities.CreateInstance<CastSyncer>(
            scope.ServiceProvider, new ShiftedTimeProvider(TimeSpan.FromDays(200)));

        var summary = await syncer.SyncAsync(CancellationToken.None);

        Assert.Equal(new CastSyncSummary(Processed: 14, Matched: 6, Failed: 0), summary);
    }

    [Fact]
    public async Task A_rejected_token_stops_the_sync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var syncer = ActivatorUtilities.CreateInstance<CastSyncer>(
            scope.ServiceProvider, new RejectingTmdbClient(), new ShiftedTimeProvider(TimeSpan.FromDays(1000)));

        await Assert.ThrowsAsync<TmdbAuthenticationException>(() => syncer.SyncAsync(CancellationToken.None));
    }

    /// <summary>"Now" plus a fixed amount, to make stored cast data look old.</summary>
    private sealed class ShiftedTimeProvider(TimeSpan shift) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + shift;
    }

    private sealed class RejectingTmdbClient : ITmdbClient
    {
        public Task<IReadOnlyList<TmdbMovie>> SearchMoviesAsync(string title, int? year, CancellationToken cancellationToken) =>
            throw new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        public Task<IReadOnlyList<TmdbCastCredit>> GetCastAsync(int tmdbMovieId, CancellationToken cancellationToken) =>
            throw new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
    }
}
