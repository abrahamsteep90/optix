using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Movies.Application.Common;
using Movies.Application.Movies;
using Movies.Domain;
using Movies.Infrastructure.Tmdb;

namespace Movies.Infrastructure.Persistence.Repositories;

internal sealed class MovieRepository(MoviesDbContext db) : IMovieRepository
{
    private static readonly Expression<Func<Movie, MovieSummaryDto>> ToSummary = m => new MovieSummaryDto(
        m.Id,
        m.Title,
        m.ReleaseDate,
        m.Overview,
        m.Popularity,
        m.VoteCount,
        m.VoteCount > 0 ? m.VoteAverage : null,
        m.OriginalLanguage,
        m.PosterUrl,
        m.Genres.OrderBy(g => g.Name).Select(g => g.Name).ToList());

    public async Task<(IReadOnlyList<MovieSummaryDto> Items, int TotalCount)> SearchAsync(
        MovieSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var movies = db.Movies.AsNoTracking();

        if (criteria.TitleContains is { } title)
        {
            var pattern = LikePattern.Contains(title);
            movies = movies.Where(m => EF.Functions.Like(m.SearchTitle, pattern, LikePattern.Escape));
        }

        foreach (var genre in criteria.Genres)
        {
            movies = movies.Where(m => m.Genres.Any(g => g.Name.ToLower() == genre));
        }

        foreach (var actor in criteria.ActorsContain)
        {
            var pattern = LikePattern.Contains(actor);
            movies = movies.Where(m =>
                m.Cast.Any(c => EF.Functions.Like(c.Actor.SearchName, pattern, LikePattern.Escape)));
        }

        var totalCount = await movies.CountAsync(cancellationToken);
        if (criteria.Skip >= totalCount)
        {
            return ([], totalCount);
        }

        var items = await Sort(movies, criteria.SortBy, criteria.SortDirection)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(ToSummary)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<MovieDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Movies
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MovieDetailsDto(
                m.Id,
                m.Title,
                m.ReleaseDate,
                m.Overview,
                m.Popularity,
                m.VoteCount,
                m.VoteCount > 0 ? m.VoteAverage : null,
                m.OriginalLanguage,
                m.PosterUrl,
                m.Genres.OrderBy(g => g.Name).Select(g => g.Name).ToList(),
                m.Cast
                    .OrderBy(c => c.BillingOrder)
                    .Select(c => new CastMemberDto(
                        c.ActorId,
                        c.Actor.Name,
                        c.Character,
                        c.Actor.ProfilePath == null ? null : TmdbImages.ProfileBaseUrl + c.Actor.ProfilePath))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

    private static IOrderedQueryable<Movie> Sort(
        IQueryable<Movie> movies, MovieSortBy sortBy, SortDirection direction)
    {
        var sorted = sortBy switch
        {
            MovieSortBy.Title => movies.OrderBy(m => m.Title, direction),
            MovieSortBy.ReleaseDate => movies.OrderBy(m => m.ReleaseDate, direction),
            // Movies nobody has voted on go last either way; equal ratings are ordered by number of votes.
            MovieSortBy.Rating => movies
                .OrderBy(m => m.VoteCount == 0)
                .ThenBy(m => m.VoteAverage, direction)
                .ThenBy(m => m.VoteCount, direction),
            _ => movies.OrderBy(m => m.Popularity, direction),
        };

        // A unique final key keeps the order stable, so paging never repeats or skips a movie
        // (e.g. the four movies called "Beauty and the Beast").
        return sorted.ThenBy(m => m.Id);
    }
}
