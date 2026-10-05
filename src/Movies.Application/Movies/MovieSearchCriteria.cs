using Movies.Application.Common;

namespace Movies.Application.Movies;

/// <summary>
/// A cleaned-up <see cref="MovieSearchQuery"/>, ready for the repository: text is already normalised,
/// blanks and duplicates are removed, and paging is turned into skip/take.
/// </summary>
public sealed record MovieSearchCriteria(
    string? TitleContains,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> ActorsContain,
    MovieSortBy SortBy,
    SortDirection SortDirection,
    int Skip,
    int Take);
