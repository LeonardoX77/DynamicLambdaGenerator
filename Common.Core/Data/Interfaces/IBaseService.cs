using FluentValidation;
using Common.Core.Data.Identity.Enums;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;
using System.Linq.Expressions;

namespace Common.Core.Data.Interfaces
{
    public interface IBaseService<TEntity, TKey>
        where TEntity : class
    {

        IQueryable<TEntity> Get<TQueryFilter>(TQueryFilter filter)
            where TQueryFilter : class, IDynamicQueryFilter, new();
        Task<IPaginatedResult<TDto>> Get<TDto, TQueryFilter>(TQueryFilter filter)
            where TQueryFilter : class, IDynamicQueryFilter, IPagination, new();

        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<TEntity> AddAsync<TDto>(TDto dto);
        Task DeleteAsync(TKey id);
        Task UpdateAsync(TEntity entity);
        Task UpdateAsync<TDto>(TDto dto) where TDto : IEntity;
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TDto>> GetAllAsync<TDto>();
        Task<TEntity> GetByPKAsync(TKey id);
        Task<TDto> GetByPKAsync<TDto>(TKey id);
        IQueryable<TDto> MapEntityToDto<TDto>(IQueryable<TEntity> searchQuery);
        TDto MapEntityToDto<TDto>(TEntity model);
        TEntity MapDtoToEntity<TDto>(TDto model, TEntity existingEntity = null);
        void ValidateDto<TDto, TValidator>(CrudAction crudAction, TDto dto)
        where TDto : IEntity
        where TValidator : AbstractValidator<TDto>;
    }
}