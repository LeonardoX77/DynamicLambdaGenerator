using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Common.Core.Generic.QueryLanguage.Interfaces;
using Common.Core.Data.Context;
using Common.Core.Data.Interfaces;

namespace Common.Core.Generic.Repository
{
    /// <summary>
    /// Base repository class.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly BaseAppDbContext _appDbContext;

        /// <inheritdoc/>
        public virtual IQueryable<TEntity> DbSet => _dbSet;

        /// <inheritdoc/>
        public IUnitOfWork UnitOfWork => _appDbContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ctx">Database context.</param>
        /// <exception cref="ArgumentNullException">Thrown when the DbSet cannot be found.</exception>
        public BaseRepository(BaseAppDbContext ctx)
        {
            _appDbContext = ctx;

            var msg = $"Failed to find a DbSet of type {nameof(TEntity)}";
            _dbSet = ctx.Set<TEntity>() ?? throw new ArgumentNullException(msg);
        }

        /// <inheritdoc/>
        public virtual TEntity Get(int id) => _dbSet.Find(id);

        /// <inheritdoc/>
        public virtual IQueryable<TEntity> Get(IDynamicExpression<TEntity> expression)
        {
            return _dbSet.Where(expression.Predicate());
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> GetAsync(int id) => await _dbSet.FindAsync(id);

        /// <inheritdoc/>
        public virtual TEntity Add(TEntity entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        /// <inheritdoc/>
        public virtual void Delete(TEntity entity)
        {
            _appDbContext.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc/>
        public virtual IQueryable<TEntity> GetAll(IPagination pagination = null)
        {
            return _dbSet.AsQueryable();
        }

        /// <inheritdoc/>
        public bool Any(Expression<Func<TEntity, bool>> expression)
            => _dbSet.Any(expression);

        /// <inheritdoc/>
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression)
            => await _dbSet.AnyAsync(expression);

        /// <inheritdoc/>
        public virtual async Task<TEntity> GetAsync(object id)
            => await _dbSet.FindAsync(id);

        /// <inheritdoc/>   
        public virtual void Update(TEntity entity)
            => _dbSet.Update(entity);

        /// <inheritdoc/>
        public async Task<bool> GetHealth()
        {
            return await _appDbContext.CanConnectAsync();
        }
    }
}
