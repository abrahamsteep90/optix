using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Movies.Application.Actors;
using Movies.Application.Genres;
using Movies.Application.Movies;
using Movies.Infrastructure.Persistence;
using Movies.Infrastructure.Persistence.Repositories;
using Movies.Infrastructure.Tmdb;

namespace Movies.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Movies";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        services.AddOptions<TmdbOptions>()
            .BindConfiguration(TmdbOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<MoviesDbContext>((provider, options) =>
        {
            var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"The connection string 'ConnectionStrings:{ConnectionStringName}' is missing.");

            options
                .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure())
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IActorRepository, ActorRepository>();
        services.AddScoped<DatabaseInitializer>();

        AddTmdb(services);

        return services;
    }

    /// <summary>Applies migrations and imports the CSV into an empty database.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(cancellationToken);
    }

    private static void AddTmdb(IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<TmdbRateLimiter>();
        services.AddTransient<RateLimitingHandler>();

        var tmdb = services.AddHttpClient<ITmdbClient, TmdbClient>((provider, http) =>
        {
            var options = provider.GetRequiredService<IOptions<TmdbOptions>>().Value;
            http.BaseAddress = options.BaseUrl;
            if (!string.IsNullOrWhiteSpace(options.ApiToken))
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
            }
        });

        // Order matters: retries (with back-off, honouring 429 Retry-After) wrap the rate limiter,
        // so every attempt, including retries, waits for its turn.
        tmdb.AddStandardResilienceHandler();
        tmdb.AddHttpMessageHandler<RateLimitingHandler>();

        services.AddScoped<CastSyncer>();
        services.AddHostedService<CastSyncBackgroundService>();
    }
}
