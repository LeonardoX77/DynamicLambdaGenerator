namespace Common.Core.Generic.Controllers
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ProducesResponseTypeNameAttribute : Attribute
    {
        public ProducesResponseTypeNameAttribute(string responseTypeName, int statusCode)
        {
            ResponseTypeName = responseTypeName;
            StatusCode = statusCode;
        }

        public string ResponseTypeName { get; }
        public int StatusCode { get; }
    }

}
