using Common.Core.CustomExceptions;

namespace Common.Core.Generic.Controllers.Response
{
    /// <summary>
    /// Response model.
    /// </summary>
    /// <typeparam name="T">Dto</typeparam>
    public class Response<T> : BaseResponse
    {
        /// <summary>
        /// Page number.
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// Maximum number of items per page.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Total number of items.
        /// </summary>
        public int? TotalRecords { get; set; }

        /// <summary>
        /// List of items.
        /// </summary>
        public List<T> Data { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Response() : base()
        {
            Data = new List<T>();
        }

        /// <summary>
        /// Constructor. OK Response Collection
        /// </summary>
        /// <param name="data">List of items.</param>
        public Response(List<T> data)
        {
            Data = data;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">List of items.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Maximum number of items per page.</param>
        /// <param name="totalRecords">Total number of items.</param>
        public Response(List<T> data, int? page, int? pageSize, int totalRecords) : this(data)
        {
            Page = page;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Item.</param>
        public Response(T data)
        {
            Data = new List<T> { data };
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <param name="errorMsg">Error message.</param>
        public Response(int errorCode, string errorMsg) : base(errorCode, errorMsg)
        {
            Data = new List<T>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="error">ApiError.</param>
        public Response(ApiError error) : base(error)
        {
            Data = new List<T>();
        }

        /// <summary>
        /// Constructor with CustomException.
        /// Created for testing purposes.
        /// </summary>
        /// <param name="exception">Custom Exception.</param>
        public Response(CustomException exception) : base(exception) { }

        /// <inheritdoc/>
        public override string ToString()
           => base.ToString();
    }
}
