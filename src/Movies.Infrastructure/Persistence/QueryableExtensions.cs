using System.Linq.Expressions;
using Movies.Application.Common;

namespace Movies.Infrastructure.Persistence;

internal static class QueryableExtensions
{
    public static IOrderedQueryable<T> OrderBy<T, TKey>(
        this IQueryable<T> source, Expression<Func<T, TKey>> key, SortDirection direction) =>
        direction == SortDirection.Asc ? source.OrderBy(key) : source.OrderByDescending(key);

    public static IOrderedQueryable<T> ThenBy<T, TKey>(
        this IOrderedQueryable<T> source, Expression<Func<T, TKey>> key, SortDirection direction) =>
        direction == SortDirection.Asc ? source.ThenBy(key) : source.ThenByDescending(key);
}
