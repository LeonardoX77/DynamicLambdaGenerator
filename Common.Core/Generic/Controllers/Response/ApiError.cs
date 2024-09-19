using System.Net;

namespace Common.Core.Generic.Controllers.Response
{
    public class ApiError
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ApiError()
        {
            Code = (int)HttpStatusCode.InternalServerError;
            Message = "Application error...";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code"></param>
        public ApiError(int code)
        {
            Code = code;
        }

        /// <summary>
        /// Constructor for handled errors
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ApiError(int code, string message) : this(code)
        {
            Message = message;
        }

        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }

        public List<string> Errors { get; set; }
    }
}
