namespace Common.Core.Data.Interfaces
{
    /// <summary>
    /// UnitOfWork Interface.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves the changes made to an entity.
        /// </summary>
        /// <returns>Integer determining if the operation was successful or not.</returns>
        int SaveChanges();

        /// <summary>
        /// Saves the changes made to an entity asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Integer determining if the operation was successful or not.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
