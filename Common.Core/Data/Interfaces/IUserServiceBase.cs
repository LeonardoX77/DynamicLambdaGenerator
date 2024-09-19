using Common.Core.Data.Identity;

namespace Common.Core.Data.Interfaces
{
    /// <summary>
    /// Interacts with ASP.NET User identity
    /// </summary>
    public interface IUserServiceBase
    {
        IQueryable<ApplicationUserBase> GetCurrentUser();
        Task<ApplicationUserBase> GetCurrentUserAsync();
    }
}