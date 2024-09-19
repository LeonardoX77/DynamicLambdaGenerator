namespace Common.Core.Data.Interfaces
{
    /// <summary>
    /// Implements necessary initialization data for the application
    /// </summary>
    public interface ISeedManager
    {
        Task CreateDefaultRolesAsync();
        Task CreateDefaultUsersAsync();
    }
}