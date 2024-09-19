using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Common.Core.Data.Identity;

namespace Common.WebApi.Infrastructure.CustomAttributes
{

    /// <summary>
    /// Custom authorization for ASP.NET Identity roles. This is a basic replacement from text to enum role names for better tracking and references in the source code
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _roles;

        public CustomAuthorizeAttribute(params ApplicationRoleType[] attributes)
        {
            _roles = GetRoles(attributes);
        }

        private string GetRoles(params ApplicationRoleType[] attributes)
        {
            return string.Join(",", attributes.Select(a => a.ToString()));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<CustomAuthorizeAttribute>>();

            logger?.LogDebug("Checking authorization...");

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                logger?.LogError("Authorization failed: user is not authenticated.");
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }

            var user = context.HttpContext.User;
            if (user == null || !_roles.Split(',').Any(user.IsInRole))
            {
                logger?.LogError($"Authorization failed: user not in role {_roles}.");
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }

            logger?.LogDebug("Authorization succeeded.");
        }
    }


}
