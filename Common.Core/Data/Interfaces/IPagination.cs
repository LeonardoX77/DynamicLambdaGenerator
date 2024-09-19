namespace Common.Core.Data.Interfaces
{
    /// <summary>
    /// Pagination and sorting model.
    /// </summary>
    public interface IPagination
    {
        /// <summary>
        /// Maximum number of items per page.
        /// </summary>
        int? PageSize { get; set; }

        /// <summary>
        /// Page number.
        /// </summary>
        int? Page { get; set; }

        /// <summary>
        /// Variable by which to sort the result set.
        /// </summary>
        string SortingFields { get; set; }

        /// <summary>
        /// Disable pagination
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// Determines the status of the request.
        /// </summary>
        /// <returns>Whether the request is valid or not.</returns>
        bool IsValid();
    }
}
