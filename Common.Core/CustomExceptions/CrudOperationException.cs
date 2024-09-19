using Common.Core.Data.Identity.Enums;
using System.Net;

namespace Common.Core.CustomExceptions
{
    /// <summary>
    /// CRUD operation exception. Thrown when there is an error performing 
    /// database operations.
    /// </summary>
    public class CrudOperationException : CustomException
    {
        public const string ERROR_CREATE = "An error occurred while adding a record to the database.";
        public const string ERROR_UPDATE = "An error occurred while trying to update a record in the database. As a result, the process was not completed.";
        public const string ERROR_DELETE = "An error occurred while trying to delete a record from the database.";
        public const string ERROR_DEFAULT = "A database error occurred, please try again later or contact support.";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="crudAction">CRUD action.</param>
        /// <param name="stackTrace">Stack trace.</param>
        /// <param name="exception">Exception.</param>
        public CrudOperationException(CrudAction crudAction, string stackTrace = "", Exception exception = null) :
            base((int)HttpStatusCode.InternalServerError, (int)crudAction,
                GetExceptionMessage(crudAction),
                stackTrace,
                exception)
        { }

        private static string GetExceptionMessage(CrudAction crudAction)
            => crudAction switch
            {
                CrudAction.CREATE => ERROR_CREATE,
                CrudAction.UPDATE => ERROR_UPDATE,
                CrudAction.DELETE => ERROR_DELETE,
                _ => ERROR_DEFAULT
            };
    }
}
