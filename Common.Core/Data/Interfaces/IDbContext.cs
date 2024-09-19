using Microsoft.EntityFrameworkCore;

namespace Common.Core.Data.Interfaces
{
    public interface IDbContext : IUnitOfWork
    {
        DbSet<T> GetDbSet<T>() where T : class;
    }
}
