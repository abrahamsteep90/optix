using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Actors;
using Movies.Application.Genres;
using Movies.Application.Movies;

namespace Movies.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IActorService, ActorService>();
        return services;
    }
}
