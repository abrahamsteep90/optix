using Movies.Application.Common;
using Movies.Domain;

namespace Movies.Application.Actors;

/// <param name="MovieCount">How many movies in the database this actor appears in.</param>
public sealed record ActorDto(int Id, string Name, int MovieCount, string? ProfileImageUrl);

/// <summary>Reads actors from storage. Implemented in the Infrastructure project.</summary>
public interface IActorRepository
{
    /// <param name="nameContains">Normalised text (see <see cref="SearchText"/>), or null for all actors.</param>
    Task<(IReadOnlyList<ActorDto> Items, int TotalCount)> SearchAsync(
        string? nameContains, int skip, int take, CancellationToken cancellationToken);
}

public interface IActorService
{
    Task<PagedResult<ActorDto>> SearchAsync(ActorSearchQuery query, CancellationToken cancellationToken);
}

public sealed class ActorService(IActorRepository repository) : IActorService
{
    public async Task<PagedResult<ActorDto>> SearchAsync(ActorSearchQuery query, CancellationToken cancellationToken)
    {
        var name = SearchText.Normalize(query.Search ?? "");

        var (items, totalCount) = await repository.SearchAsync(
            name.Length == 0 ? null : name,
            skip: (query.Page - 1) * query.PageSize,
            take: query.PageSize,
            cancellationToken);

        return new PagedResult<ActorDto>(items, query.Page, query.PageSize, totalCount);
    }
}
