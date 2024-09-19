using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Common.Core.Data.Context;
using Common.Core.Data.Identity;
using Common.Core.Data.Interfaces;
using System.Security.Claims;

namespace Common.Core.Generic.Services
{
    public class UserServiceBase : IUserServiceBase
    {
        private readonly UserManager<ApplicationUserBase> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BaseAppDbContext _dbContext;

        public UserServiceBase(
            UserManager<ApplicationUserBase> userManager,
            IHttpContextAccessor httpContextAccessor,
            BaseAppDbContext dbContext)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<ApplicationUserBase> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId);
        }

        public IQueryable<ApplicationUserBase> GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return null;
            }

            return _dbContext.Users.Where(u => u.Id == userId);
        }
    }

}
