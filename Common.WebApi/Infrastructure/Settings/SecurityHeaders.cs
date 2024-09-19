namespace Common.WebApi.Infrastructure.Settings
{
    /// <summary>
    /// Class to collect security headers in the appSettings.
    /// </summary>
    public class SecurityHeaders
    {
        /// <summary>
        /// Dictionary of - security header, header value -
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
    }
}
