using Common.WebApi.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Common.WebApi.Application.Middlewares
{
    /// <summary>
    /// Adds different security headers required by the infrastructure
    /// </summary>
    public class SecurityHeadersMiddleware : IMiddleware
    {
        private readonly SecurityHeaders _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Configuracion de SecurityHeaders.</param>
        public SecurityHeadersMiddleware(IOptions<SecurityHeaders> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;

                foreach (var header in _options.Headers)
                {
                    httpContext.Response.Headers.Append(header.Key, header.Value);
                }

                return Task.CompletedTask;
            }, context);

            return next(context);
        }
    }
}
