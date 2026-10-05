using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Actors;

public sealed class ActorSearchQuery
{
    /// <summary>Part of the actor's name. Case and accents are ignored.</summary>
    [StringLength(200)]
    public string? Search { get; init; }

    /// <summary>Page number, starting at 1.</summary>
    [Range(1, 1_000_000)]
    public int Page { get; init; } = 1;

    /// <summary>Results per page (1–100). Defaults to 20.</summary>
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
