namespace Movies.Application.Genres;

/// <param name="MovieCount">How many movies have this genre.</param>
public sealed record GenreDto(int Id, string Name, int MovieCount);

/// <summary>Reads genres from storage. Implemented in the Infrastructure project.</summary>
public interface IGenreRepository
{
    Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken);
}

public interface IGenreService
{
    Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken);
}

public sealed class GenreService(IGenreRepository repository) : IGenreService
{
    public Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}
