using System;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Data.Helpers.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, bool when,
            Expression<Func<TSource, bool>> then, Expression<Func<TSource, bool>> @else = null)
        {
            if (when)
            {
                return source.Where(then);
            }
            else if (@else != null)
            {
                return source.Where(@else);
            }

            return source;
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource, TProp>(this IQueryable<TSource> query, ISortParams sortParams, string prop, Expression<Func<TSource, TProp>> expression)
        {
            if (sortParams == null || !sortParams.SortBy.Any(x => string.Compare(x, prop, true) == 0))
            {
                return (IOrderedQueryable<TSource>)query;
            }

            var index = Array.FindIndex(sortParams.SortBy, x => string.Compare(x, prop, true) == 0);
            var dir = index > sortParams.SortDir.Length ? SortDirs.Asc : sortParams.SortDir[index];
            if (dir == SortDirs.Asc)
            {
                return query.OrderBy(expression);
            }
            else
            {
                return query.OrderByDescending(expression);
            }
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource, TProp>(this IOrderedQueryable<TSource> query, ISortParams sortParams, string prop, Expression<Func<TSource, TProp>> expression)
        {
            return query.AsQueryable().OrderBy(sortParams, prop, expression);
        }
    }
}
