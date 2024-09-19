namespace Common.Core.Data.Identity
{
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// This is the appication user. We can define here custom fields that will be included into the IdentityUser model and persisted in Database
    /// </summary>
    public class ApplicationUserBase : IdentityUser
    {

    }
}
