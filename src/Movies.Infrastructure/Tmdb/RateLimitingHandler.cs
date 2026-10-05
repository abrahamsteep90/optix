using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Movies.Infrastructure.Tmdb;

/// <summary>One limiter shared by every TMDB request, so the whole app stays under the requests-per-second limit.</summary>
internal sealed class TmdbRateLimiter(IOptions<TmdbOptions> options) : IDisposable
{
    private readonly TokenBucketRateLimiter _limiter = new(new TokenBucketRateLimiterOptions
    {
        TokenLimit = options.Value.RequestsPerSecond,
        TokensPerPeriod = options.Value.RequestsPerSecond,
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        QueueLimit = int.MaxValue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        AutoReplenishment = true,
    });

    public ValueTask<RateLimitLease> WaitAsync(CancellationToken cancellationToken) =>
        _limiter.AcquireAsync(1, cancellationToken);

    public void Dispose() => _limiter.Dispose();
}

/// <summary>Waits for a slot from <see cref="TmdbRateLimiter"/> before each request, including retries.</summary>
internal sealed class RateLimitingHandler(TmdbRateLimiter limiter) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await limiter.WaitAsync(cancellationToken);
        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException("Could not get a TMDB rate-limit slot.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
