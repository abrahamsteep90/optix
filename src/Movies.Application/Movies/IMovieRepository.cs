namespace Movies.Application.Movies;

/// <summary>Reads movies from storage. Implemented in the Infrastructure project.</summary>
public interface IMovieRepository
{
    Task<(IReadOnlyList<MovieSummaryDto> Items, int TotalCount)> SearchAsync(
        MovieSearchCriteria criteria, CancellationToken cancellationToken);

    Task<MovieDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
