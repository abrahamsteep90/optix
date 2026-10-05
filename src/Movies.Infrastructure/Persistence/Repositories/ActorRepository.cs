using Microsoft.EntityFrameworkCore;
using Movies.Application.Actors;
using Movies.Infrastructure.Tmdb;

namespace Movies.Infrastructure.Persistence.Repositories;

internal sealed class ActorRepository(MoviesDbContext db) : IActorRepository
{
    public async Task<(IReadOnlyList<ActorDto> Items, int TotalCount)> SearchAsync(
        string? nameContains, int skip, int take, CancellationToken cancellationToken)
    {
        var actors = db.Actors.AsNoTracking();

        if (nameContains is not null)
        {
            var pattern = LikePattern.Contains(nameContains);
            actors = actors.Where(a => EF.Functions.Like(a.SearchName, pattern, LikePattern.Escape));
        }

        var totalCount = await actors.CountAsync(cancellationToken);
        if (skip >= totalCount)
        {
            return ([], totalCount);
        }

        // Actors in the most movies first: that is usually who someone typing "chris" is looking for.
        var items = await actors
            .OrderByDescending(a => a.Roles.Count)
            .ThenBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Skip(skip)
            .Take(take)
            .Select(a => new ActorDto(
                a.Id,
                a.Name,
                a.Roles.Count,
                a.ProfilePath == null ? null : TmdbImages.ProfileBaseUrl + a.ProfilePath))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
