namespace Movies.Application.Movies;

/// <summary>A movie as it appears in a list of results.</summary>
/// <param name="VoteAverage">Average vote out of 10, or null when nobody has voted yet.</param>
public sealed record MovieSummaryDto(
    int Id,
    string Title,
    DateOnly ReleaseDate,
    string Overview,
    double Popularity,
    int VoteCount,
    decimal? VoteAverage,
    string OriginalLanguage,
    string PosterUrl,
    IReadOnlyList<string> Genres);

/// <summary>A single movie with its full details and cast.</summary>
/// <param name="VoteAverage">Average vote out of 10, or null when nobody has voted yet.</param>
/// <param name="Cast">Top-billed actors, in billing order. Empty when cast data has not been loaded.</param>
public sealed record MovieDetailsDto(
    int Id,
    string Title,
    DateOnly ReleaseDate,
    string Overview,
    double Popularity,
    int VoteCount,
    decimal? VoteAverage,
    string OriginalLanguage,
    string PosterUrl,
    IReadOnlyList<string> Genres,
    IReadOnlyList<CastMemberDto> Cast);

/// <summary>An actor's role in a movie.</summary>
public sealed record CastMemberDto(int ActorId, string Name, string? Character, string? ProfileImageUrl);
