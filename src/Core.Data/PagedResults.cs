using System.Collections.Immutable;

namespace Core.Data
{
    public class PagedResults<T>
    {
        public PagedResults(IImmutableList<T> results, int total)
        {
            Results = results;
            Total = total;
        }

        public int Total { get; set; }
        public IImmutableList<T> Results { get; }
    }

    public static class PagedResults
    {
        public static PagedResults<T> Create<T>(IImmutableList<T> results, int total)
        {
            return new PagedResults<T>(results, total);
        }
    }
}
