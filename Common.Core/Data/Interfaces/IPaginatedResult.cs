namespace Common.Core.Data.Interfaces
{
    public interface IPaginatedResult<T>
    {
        IEnumerable<T> Items { get; set; }
        int Page { get; set; }
        int PageSize { get; set; }
        int TotalCount { get; set; }
    }

}