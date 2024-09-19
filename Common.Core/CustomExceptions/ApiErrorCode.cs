namespace Common.Core.CustomExceptions
{
    /// <summary>
    /// Custom error codes
    /// </summary>
    public enum ApiErrorCode
    {
        BAD_REQUEST = 400,
        INTERNAL_SERVER_ERROR = 500,
        UNAUTHORIZED = 401,
        NOT_FOUND = 404,

        NO_RECORD = 1001,
        DB_RECORD_ALREADY_EXISTS = 1002,
        STRING_MAX_LENGHT = 1003,
        REQUIRED_FIELD = 1004,
        OUT_OF_RANGE_DATE = 1005,
        NO_RECORDS = 1006,
        STRING_MAX_LENGTH = 1007,
        INVALID_EMAIL_FORMAT = 1008,
        INVALID_AGE_UNDER_18 = 1009,
    }
}
