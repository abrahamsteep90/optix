using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Movies.Infrastructure.Tmdb;

/// <summary>
/// Runs <see cref="CastSyncer"/> in the background once the app has started, then once a day
/// (to refresh old cast data). Does nothing without a TMDB token. The API is fully usable meanwhile.
/// </summary>
internal sealed class CastSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<TmdbOptions> options,
    ILogger<CastSyncBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiToken))
        {
            logger.LogInformation(
                "Cast sync is off: no TMDB token is configured (Tmdb:ApiToken). Everything else works, " +
                "but filtering by actor will find nothing");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var summary = await scope.ServiceProvider.GetRequiredService<CastSyncer>().SyncAsync(stoppingToken);
                if (summary.Processed > 0)
                {
                    logger.LogInformation(
                        "Cast sync finished: {Matched} of {Processed} movies found on TMDB, {Failed} failed",
                        summary.Matched, summary.Processed, summary.Failed);
                }
            }
            catch (TmdbAuthenticationException)
            {
                logger.LogError(
                    "TMDB rejected the token (401 Unauthorized). Use the 'API Read Access Token' from " +
                    "https://www.themoviedb.org/settings/api, not the shorter 'API Key'. Cast sync has stopped");
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Cast sync failed; it will try again tomorrow");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
