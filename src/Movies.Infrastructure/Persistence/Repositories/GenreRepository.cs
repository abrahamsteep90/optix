using Microsoft.EntityFrameworkCore;
using Movies.Application.Genres;

namespace Movies.Infrastructure.Persistence.Repositories;

internal sealed class GenreRepository(MoviesDbContext db) : IGenreRepository
{
    public async Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GenreDto(g.Id, g.Name, g.Movies.Count))
            .ToListAsync(cancellationToken);
}
