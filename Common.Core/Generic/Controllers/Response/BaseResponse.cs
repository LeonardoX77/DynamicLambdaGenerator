using Common.Core.CustomExceptions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Core.Generic.Controllers.Response
{


    /// <summary>
    /// A common response for all responses.
    /// </summary>
    public class BaseResponse
    {
        private static readonly HashSet<int> InvalidStatusCodes = new HashSet<int>(Enum.GetValues(typeof(ApiErrorCode)).Cast<int>());

        /// <summary>
        /// ApiError.
        /// </summary>
        public ApiError Error { get; set; } = null;
        public int? StatusCode { get; set; }

        private bool _success = false;
        public bool Success
        {
            get
            {
                return !StatusCode.HasValue || _success || !InvalidStatusCodes.Contains(StatusCode.Value);
            }
            set
            {
                _success = value;
            }
        }


        public string Message { get; set; }

        /// <summary>
        /// Constructor for generic errors
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        public BaseResponse(int errorCode, string errorMsg = "")
        {
            Error = new ApiError(errorCode, errorMsg);
            StatusCode = errorCode;
        }

        /// <summary>
        /// Constructor using already formed errors
        /// </summary>
        /// <param name="error"></param>
        public BaseResponse(ApiError error)
        {
            Error = error;
            StatusCode = error.Code;
        }

        /// <summary>
        /// Contructor with CustomExceptions.
        /// </summary>
        /// <param name="exception">Custom Exception.</param>
        public BaseResponse(CustomException exception)
        {
            Error = new ApiError(
                exception.ErrorCode,
                exception.Message);
            StatusCode = exception.HttpStatusCodeResponse;
        }

        public BaseResponse() { }

        /// <summary>
        /// Determies wether response contains an error or not.
        /// </summary>
        /// <returns></returns>
        public bool HasError()
            => Error != null && Error.Code != (int)HttpStatusCode.OK;

        /// <summary>
        /// Serializes this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
           => JsonSerializer.Serialize(this);
    }
}
