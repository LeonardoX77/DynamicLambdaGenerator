namespace Common.Core.Generic.Controllers
{

    /// <summary>
    /// Attribute to specify a generic response type for Swagger documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ProducesGenericResponseTypeAttribute : Attribute
    {
        public Type ResponseType { get; }
        public string TypeArgumentName { get; }
        public int StatusCode { get; }

        public ProducesGenericResponseTypeAttribute(Type responseType, string typeArgumentName, int statusCode)
        {
            ResponseType = responseType;
            TypeArgumentName = typeArgumentName;
            StatusCode = statusCode;
        }
    }
}
