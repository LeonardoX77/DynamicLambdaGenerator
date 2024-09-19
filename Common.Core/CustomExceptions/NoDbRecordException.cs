using System.Net;

namespace Common.Core.CustomExceptions
{
    public class NoDbRecordException : CustomException
    {
        public NoDbRecordException(string entityName, string propertyName, string propertyValue, string stackTrace = "") :
            base(
                (int)HttpStatusCode.NotFound,
                $"Entity: {entityName} with {propertyName}: ({propertyValue}) was not found.")
        {
            ErrorCode = (int)ApiErrorCode.NO_RECORD;
            ErrorTag = nameof(ApiErrorCode.NO_RECORD);
        }
    }
}
