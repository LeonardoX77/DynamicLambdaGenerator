using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Common.Core.Generic.Controllers
{
    /// <summary>
    /// Operation filter to apply the generic response type in Swagger documentation.
    /// This filter retrieves the actual type of the generic response type from the controller and combines it with the specified generic response model.
    /// 
    /// Steps:
    /// 1. The attribute <see cref="ProducesGenericResponseTypeAttribute"/> is applied to the controller method and specifies the generic response type (e.g., <see cref="Response{T}"/>).
    /// 2. In this filter, we retrieve the controller type and determine the actual type argument for the generic type (e.g., <see cref="ClientResponseDto"/>).
    /// 3. Using reflection, the filter constructs the specific response type (e.g., <see cref="Response{ClientResponseDto}"/>).
    /// 4. The filter then updates the Swagger operation documentation to reflect the specific response type and status code.
    /// 
    /// This allows the use of generic response types in controller methods while still providing accurate Swagger documentation.
    /// </summary>
    public class ProducesResponseTypeGenericFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(true)
                .OfType<ProducesGenericResponseTypeAttribute>()
                .FirstOrDefault();

            if (attribute == null) return;

            var controllerType = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ApiDescription.ActionDescriptor).ControllerTypeInfo.AsType();
            var genericTypeArgument = controllerType.BaseType?.GetGenericArguments()
                .FirstOrDefault(x => x.Name.EndsWith(attribute.TypeArgumentName.Substring(1)));

            if (genericTypeArgument != null)
            {
                var responseType = attribute.ResponseType.MakeGenericType(genericTypeArgument);
                operation.Responses[attribute.StatusCode.ToString()] = new OpenApiResponse
                {
                    //Description = "Successful response",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository)
                        }
                    }
                };
            }
        }
    }

}
