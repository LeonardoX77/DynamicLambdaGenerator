using Common.Core.Data.Interfaces;

namespace Common.WebApi.Infrastructure.Models.Request
{
    /// <summary>
    /// Base request that implements pagination and sorting.
    /// </summary>
    public class BaseRequest : IPagination
    {
        private const int DEFAULT_PAGE_SIZE = 50;
        private const int DEFAULT_PAGE = 1;
        private int? _page;
        private int? _pageSize;

        /// <summary>
        /// Page number, should never be null or 0.
        /// Returns 1 if the value is null or 0.
        /// </summary>
        public int? Page
        {
            get => _page.GetValueOrDefault(1) < 1 ? DEFAULT_PAGE : _page ?? DEFAULT_PAGE;
            set => _page = value;
        }

        /// <summary>
        /// Page size, should never be null or 0.
        /// Returns 50 if the value is null or 0.
        /// </summary>
        public int? PageSize
        {
            get => _pageSize.GetValueOrDefault(50) < 1 ? DEFAULT_PAGE_SIZE : _pageSize ?? DEFAULT_PAGE_SIZE;
            set => _pageSize = value;
        }

        /// <inheritdoc />
        public string SortingFields { get; set; }

        /// <inheritdoc />
        public bool Disabled { get; set; }

        /// <inheritdoc />
        public bool IsValid() => Page.HasValue && PageSize.HasValue;
    }
}
