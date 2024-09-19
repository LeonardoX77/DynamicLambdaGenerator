using Common.Core.Data.Interfaces;

namespace Common.Core.Data.Wrappers
{
    /// <summary>
    /// Paginated data returned to Http Clients
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedResult<T> : IPaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PaginatedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}