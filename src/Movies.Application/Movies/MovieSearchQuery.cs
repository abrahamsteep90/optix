using System.ComponentModel.DataAnnotations;
using Movies.Application.Common;

namespace Movies.Application.Movies;

/// <summary>What a client can ask for when listing movies. Every part is optional.</summary>
public sealed class MovieSearchQuery
{
    public const int MaxPageSize = 100;

    /// <summary>Part of the title to look for. Case and accents are ignored ("pokemon" finds "Pokémon").</summary>
    [StringLength(200)]
    public string? Search { get; init; }

    /// <summary>Only movies that have all of these genres. Repeat the parameter for more than one genre.</summary>
    [MaxLength(10)]
    public string[] Genres { get; init; } = [];

    /// <summary>Only movies with all of these actors. A value can be part of a name, e.g. "hanks". Repeat the parameter for more than one actor.</summary>
    [MaxLength(10)]
    public string[] Actors { get; init; } = [];

    /// <summary>What to sort by. Defaults to popularity.</summary>
    [EnumDataType(typeof(MovieSortBy))]
    public MovieSortBy SortBy { get; init; } = MovieSortBy.Popularity;

    /// <summary>Defaults to A–Z for title and to highest/newest first for everything else.</summary>
    [EnumDataType(typeof(SortDirection))]
    public SortDirection? SortDirection { get; init; }

    /// <summary>Page number, starting at 1.</summary>
    [Range(1, 1_000_000)]
    public int Page { get; init; } = 1;

    /// <summary>Results per page (1–100). Defaults to 20.</summary>
    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = 20;
}
