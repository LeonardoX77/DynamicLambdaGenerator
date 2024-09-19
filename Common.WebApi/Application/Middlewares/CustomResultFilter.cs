using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Common.Core.CustomExceptions;
using System.Net;
using Common.Core.Generic.Controllers.Response;

namespace Common.WebApi.Application.Middlewares
{
    /// <summary>
    /// Intercepts when the model state is invalid.
    /// The ModelStateInvalidFilter (part of the MVC pipeline)
    /// produces a BadRequestObjectResult with the details
    /// of validation errors.
    /// </summary>
    public class CustomResultFilter : IResultFilter
    {
        /// <inheritdoc/>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is BadRequestObjectResult badRequestResult)
            {
                BaseResponse response;

                // Checks if the value is ValidationProblemDetails
                if (badRequestResult.Value is ValidationProblemDetails validationProblemDetails)
                {
                    string errorMessages = validationProblemDetails.Errors
                        .SelectMany(pair => pair.Value)
                        .FirstOrDefault();

                    response = new BaseResponse(
                        badRequestResult.StatusCode.Value,
                        $"{validationProblemDetails.Title} {errorMessages}");
                }
                // Checks if the value is SerializableError
                else if (badRequestResult.Value is SerializableError serializableError)
                {
                    var errorMessages = serializableError
                        .SelectMany(pair => pair.Value as string[])
                        .Aggregate(string.Empty, (current, next) => current + next + "; ");

                    response = new BaseResponse(
                        badRequestResult.StatusCode.Value,
                        $"Bad Request: {errorMessages.TrimEnd(' ', ';')}");
                }
                else
                {
                    // If the object type is none of the above, handle it as unknown
                    response = new BaseResponse(
                        badRequestResult.StatusCode.Value,
                        "Bad Request: Unknown error");
                }

                context.Result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <inheritdoc/>
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // do nothing.
        }
    }
}
