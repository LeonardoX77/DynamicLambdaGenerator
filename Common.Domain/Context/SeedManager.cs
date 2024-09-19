using Microsoft.AspNetCore.Identity;
using Common.Core.Data.Identity;
using Common.Core.Data.Interfaces;

namespace Common.Business.Services.Common
{
    /// <summary>
    /// Manages the seeding of default roles and users and other needed initialization data.
    /// </summary>
    public class SeedManager : ISeedManager
    {
        private readonly UserManager<ApplicationUserBase> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userManager">User manager service.</param>
        /// <param name="roleManager">Role manager service.</param>
        public SeedManager(
            UserManager<ApplicationUserBase> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates default roles if they do not exist.
        /// </summary>
        public async Task CreateDefaultRolesAsync()
        {
            ApplicationRoleType[] roleNames = { ApplicationRoleType.Admin, ApplicationRoleType.User };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName.ToString()))
                {
                    var role = new ApplicationRole(roleName.ToString());
                    await _roleManager.CreateAsync(role);
                }
            }
        }

        /// <summary>
        /// Creates default users if they do not exist.
        /// </summary>
        public async Task CreateDefaultUsersAsync()
        {
            string[] userEmails = { "admin@mydomain.com", "user@mydomain.com" };
            foreach (var userEmail in userEmails)
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    user = new ApplicationUserBase { UserName = userEmail, Email = userEmail };
                    var pwd = $"${user.Email.Split("@")[0]}@mydomain2222X";
                    var result = await _userManager.CreateAsync(user, pwd);

                    await _userManager.AddToRoleAsync(user, userEmail.Contains("admin") ?
                        ApplicationRoleType.Admin.ToString() :
                        ApplicationRoleType.User.ToString());
                }
            }
        }
    }
}
