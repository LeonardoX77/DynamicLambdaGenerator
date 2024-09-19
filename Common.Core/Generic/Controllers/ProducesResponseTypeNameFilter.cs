using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Common.Core.Generic.Controllers
{
    /// <summary>
    /// An action filter that dynamically sets the response type for a controller action 
    /// based on a specified type name. This allows for more flexible and generic 
    /// response type definitions in ASP.NET Core controllers.
    /// 
    /// The filter retrieves the actual type for the response based on the provided 
    /// type name and sets it accordingly, enabling accurate API documentation 
    /// generation with tools like Swagger.
    /// </summary>
    public class ProducesResponseTypeNameFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            var attribute = methodInfo.GetCustomAttribute<ProducesResponseTypeNameAttribute>();
            if (attribute != null)
            {
                var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                if (controllerActionDescriptor != null)
                {
                    var controllerType = controllerActionDescriptor.ControllerTypeInfo.AsType();
                    var genericArguments = controllerType.BaseType?.GetGenericArguments();
                    if (genericArguments != null && genericArguments.Length > 2)
                    {
                        var responseTypeName = genericArguments[2].FullName;
                        var responseType = controllerType.Assembly.GetType(responseTypeName);
                        if (responseType != null)
                        {
                            var schema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository);
                            operation.Responses[attribute.StatusCode.ToString()] = new OpenApiResponse
                            {
                                Description = "Successful response",
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = schema
                                    }
                                }
                            };
                        }
                    }
                }
            }
        }
    }


}
