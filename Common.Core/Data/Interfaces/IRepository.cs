using Common.Core.Generic.QueryLanguage.Interfaces;
using System.Linq.Expressions;

namespace Common.Core.Data.Interfaces
{
    /// <summary>
    /// Base repository interface.
    /// </summary>
    /// <typeparam name="TEntity">Entity</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        IUnitOfWork UnitOfWork { get; }
        IQueryable<TEntity> DbSet { get; }
        IQueryable<TEntity> GetAll(IPagination pagination = null);
        TEntity Add(TEntity entity);
        void Delete(TEntity id);
        void Update(TEntity entity);
        bool Any(Expression<Func<TEntity, bool>> expression);
        Task<TEntity> GetAsync(object id);
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression);
        IQueryable<TEntity> Get(IDynamicExpression<TEntity> expression);
        TEntity Get(int id);
        Task<bool> GetHealth();
    }
}
