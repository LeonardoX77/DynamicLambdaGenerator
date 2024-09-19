namespace Common.Core.Data.Identity
{
    using Microsoft.AspNetCore.Identity;

    public enum ApplicationRoleType
    {
        User = 0,
        Admin = 1
    }

    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
        public ApplicationRole() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }

}
