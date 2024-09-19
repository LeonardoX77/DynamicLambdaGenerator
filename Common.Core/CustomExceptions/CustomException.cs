using System.Net;

namespace Common.Core.CustomExceptions
{
    /// <summary>
    /// Generic exception. This class only serves to type custom exceptions with the same base
    /// and to differentiate them from an uncontrolled exception in the GlobalExceptionHandler.
    /// The difference between them is that our exceptions carry their own message, while uncontrolled exceptions
    /// carry a trace that should not be returned to the front for security reasons and therefore it is logged and replaced with a generic error message.
    /// </summary>
    public class CustomException : Exception
    {
        /// <summary>
        /// Error code.
        /// </summary>
        public int ErrorCode { get; protected set; }

        /// <summary>
        /// Error code in string format. For use in Front.
        /// </summary>
        public string ErrorTag { get; protected set; }

        /// <summary>
        /// The HTTP Status Code that the exception returns
        /// when it is thrown.
        /// </summary>
        public int HttpStatusCodeResponse { get; }
        public string ErrorStackTrace { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpStatusCode">HTTP Status Code.</param>
        /// <param name="msg">Message.</param>
        /// <param name="trace">Trace.</param>
        /// <param name="inner">Inner exception.</param>
        public CustomException(int httpStatusCode, string msg, Exception inner = null) : base(msg, inner)
        {
            ErrorCode = (int)HttpStatusCode.InternalServerError;
            ErrorTag = nameof(ApiErrorCode.INTERNAL_SERVER_ERROR);
            HttpStatusCodeResponse = httpStatusCode;

        }

        public override string StackTrace => ErrorStackTrace ?? base.StackTrace;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpStatusCode">HTTP Status Code.</param>
        /// <param name="code">Custom code for the front.</param>
        /// <param name="msg">Message.</param>
        /// <param name="stackTrace">Trace.</param>
        /// <param name="inner">Inner exception.</param>
        public CustomException(int httpStatusCode, int code, string msg, string stackTrace, Exception inner = null) : base(msg, inner)
        {
            ErrorCode = code;
            ErrorTag = ((ApiErrorCode)code).ToString();
            HttpStatusCodeResponse = httpStatusCode;
            ErrorStackTrace = stackTrace;
        }

        /// <summary>
        /// Determines if there is an inner exception or not.
        /// </summary>
        /// <returns>Whether there is an inner exception or not.</returns>
        public bool HasInnerException() => InnerException != null;
    }
}
