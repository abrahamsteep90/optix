using Movies.Application.Common;
using Movies.Domain;

namespace Movies.Application.Movies;

public interface IMovieService
{
    Task<PagedResult<MovieSummaryDto>> SearchAsync(MovieSearchQuery query, CancellationToken cancellationToken);

    Task<MovieDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}

public sealed class MovieService(IMovieRepository repository) : IMovieService
{
    public async Task<PagedResult<MovieSummaryDto>> SearchAsync(
        MovieSearchQuery query, CancellationToken cancellationToken)
    {
        var criteria = new MovieSearchCriteria(
            TitleContains: NullIfEmpty(SearchText.Normalize(query.Search ?? "")),
            Genres: CleanList(query.Genres),
            ActorsContain: CleanList(query.Actors),
            SortBy: query.SortBy,
            SortDirection: query.SortDirection ?? DefaultDirection(query.SortBy),
            Skip: (query.Page - 1) * query.PageSize,
            Take: query.PageSize);

        var (items, totalCount) = await repository.SearchAsync(criteria, cancellationToken);

        return new PagedResult<MovieSummaryDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<MovieDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    /// <summary>Titles read best A–Z; for numbers and dates people usually want the top or newest first.</summary>
    public static SortDirection DefaultDirection(MovieSortBy sortBy) =>
        sortBy == MovieSortBy.Title ? SortDirection.Asc : SortDirection.Desc;

    private static string[] CleanList(IEnumerable<string> values) =>
        values.Select(SearchText.Normalize).Where(v => v.Length > 0).Distinct().ToArray();

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;
}
