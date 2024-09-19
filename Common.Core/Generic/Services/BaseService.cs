using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Common.Core.CustomExceptions;
using Common.Core.Data.Identity.Enums;
using Common.Core.Data.Interfaces;
using Common.Core.Data.Wrappers;
using Common.Core.Generic.QueryLanguage;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.Core.Generic.Services
{
    /// <summary>
    /// Base service class providing common CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">Entity key type.</typeparam>
    public class BaseService<TEntity, TKey> : IBaseService<TEntity, TKey>
        where TEntity : class, new()
        where TKey : struct
    {
        protected readonly IRepository<TEntity> _baseRepo;
        protected readonly IMapper _mapper;
        protected readonly IValidationService _validationService;
        private readonly DynamicFiltersConfiguration _dynamicFiltersConfiguration;

        /// <summary>
        /// Constructor for BaseService.
        /// </summary>
        /// <param name="baseRepo">Repository for entity operations.</param>
        /// <param name="validationService">Service for validating DTOs.</param>
        /// <param name="mapper">Mapper for transforming entities and DTOs.</param>
        /// <param name="config">Configuration for dynamic filters.</param>
        public BaseService(
            IRepository<TEntity> baseRepo,
            IValidationService validationService,
            IMapper mapper,
            IOptions<DynamicFiltersConfiguration> config)
        {
            _baseRepo = baseRepo;
            _mapper = mapper;
            _validationService = validationService;
            _dynamicFiltersConfiguration = config.Value;
        }

        /// <summary>
        /// Adds an entity asynchronously.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <param name="dto">DTO instance.</param>
        /// <returns>The created entity.</returns>
        public virtual async Task<TEntity> AddAsync<TDto>(TDto dto)
        {
            TEntity entity = MapDtoToEntity(dto);
            return await AddAsync(entity);
        }

        /// <summary>
        /// Adds an entity asynchronously.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>The created entity.</returns>
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            var created = await _baseRepo.AddAsync(entity);
            await SaveChangesAsync(CrudAction.CREATE);
            return created;
        }

        /// <summary>
        /// Adds a range of entities asynchronously.
        /// </summary>
        /// <param name="entities">Entities to add.</param>
        /// <returns>The added entities.</returns>
        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            IEnumerable<TEntity> created = await _baseRepo.AddRangeAsync(entities);
            await SaveChangesAsync(CrudAction.CREATE);
            return created;
        }

        /// <summary>
        /// Saves changes asynchronously.
        /// </summary>
        /// <param name="crudAction">The CRUD action performed.</param>
        private async Task SaveChangesAsync(CrudAction crudAction)
        {
            try
            {
                await _baseRepo.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new CrudOperationException(crudAction, ex.StackTrace, ex.InnerException);
            }
        }

        /// <summary>
        /// Deletes an entity asynchronously.
        /// </summary>
        /// <param name="id">ID of the entity to delete.</param>
        public virtual async Task DeleteAsync(TKey id)
        {
            TEntity entity = await _baseRepo.GetAsync(id) ?? throw new NoDbRecordException(nameof(TEntity), nameof(id), id.ToString());
            _baseRepo.Delete(entity);
            await SaveChangesAsync(CrudAction.DELETE);
        }

        /// <summary>
        /// Updates an entity asynchronously.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            _baseRepo.Update(entity);
            await SaveChangesAsync(CrudAction.UPDATE);
        }

        /// <summary>
        /// Updates an entity asynchronously from a DTO.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <param name="dto">DTO instance.</param>
        public virtual async Task UpdateAsync<TDto>(TDto dto) where TDto : IEntity
        {
            TEntity existingEntity = await _baseRepo.GetAsync(dto.Id) ?? throw new NoDbRecordException(nameof(TEntity), nameof(dto.Id), dto.Id.ToString());
            TEntity entity = MapDtoToEntity(dto, existingEntity);
            await UpdateAsync(entity);
        }

        /// <summary>
        /// Gets entities based on a filter.
        /// </summary>
        /// <typeparam name="TQueryFilter">Filter type.</typeparam>
        /// <param name="filter">Filter instance.</param>
        /// <returns>Queryable of entities.</returns>
        public virtual IQueryable<TEntity> Get<TQueryFilter>(TQueryFilter filter)
            where TQueryFilter : class, IDynamicQueryFilter, new()
        {
            IQueryable<TEntity> query = _baseRepo.Get(
                new DynamicExpression<TEntity, TQueryFilter>(filter, _dynamicFiltersConfiguration));

            return query;
        }

        /// <summary>
        /// Creates a dynamic IQueryable<TEntity> and projects to IEnumerable<TDto> during database query execution.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <typeparam name="TQueryFilter">Filter type.</typeparam>
        /// <param name="filter">Filter instance.</param>
        /// <returns>Paginated result of DTOs.</returns>
        public virtual async Task<IPaginatedResult<TDto>> Get<TDto, TQueryFilter>(TQueryFilter filter)
            where TQueryFilter : class, IDynamicQueryFilter, IPagination, new()
        {
            //Get queryable with dynamic lambda expressions
            IQueryable<TEntity> query = Get(filter);

            //get count before pagination
            int totalCount = await query.CountAsync();

            //apply sorting
            query = QueryableHelper<TEntity>.ApplySorting(query, filter);

            //apply pagination
            query = ApplyPagination(filter, query);

            //Get data and transform to DTO in one single operation
            List<TDto> results = await MapEntityToDto<TDto>(query).ToListAsync();

            return new PaginatedResult<TDto>(results, totalCount, filter.Page.Value, filter.PageSize.Value);
        }

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <returns>All entities.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _baseRepo.GetAll().ToListAsync();
        }

        /// <summary>
        /// Gets all entities transformed to DTO type.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <returns>All entities transformed to DTO type.</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllAsync<TDto>()
        {
            var query = _baseRepo.GetAll();
            return await MapEntityToDto<TDto>(query).ToListAsync();
        }

        /// <summary>
        /// Gets an entity by its primary key asynchronously.
        /// </summary>
        /// <param name="id">ID of the entity.</param>
        /// <returns>The entity.</returns>
        public virtual async Task<TEntity> GetByPKAsync(TKey id)
        {
            return await _baseRepo.GetAsync(id);
        }

        /// <summary>
        /// Gets an entity by its primary key transformed to DTO type asynchronously.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <param name="id">ID of the entity.</param>
        /// <returns>The entity transformed to DTO type.</returns>
        public virtual async Task<TDto> GetByPKAsync<TDto>(TKey id)
        {
            return MapEntityToDto<TDto>(await _baseRepo.GetAsync(id));
        }

        /// <summary>
        /// Maps a DTO to an entity.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <param name="model">DTO instance.</param>
        /// <param name="existingEntity">Existing entity instance.</param>
        /// <returns>Mapped entity.</returns>
        public TEntity MapDtoToEntity<TDto>(TDto model, TEntity existingEntity = null)
        {
            if (existingEntity == null)
                return _mapper.Map<TDto, TEntity>(model);
            else
                return _mapper.Map(model, existingEntity);
        }

        /// <summary>
        /// Maps an entity to a DTO.
        /// </summary>
        /// <typeparam name="TOutput">DTO type.</typeparam>
        /// <param name="model">Entity instance.</param>
        /// <returns>Mapped DTO.</returns>
        public TOutput MapEntityToDto<TOutput>(TEntity model)
        {
            return _mapper.Map<TEntity, TOutput>(model);
        }

        /// <summary>
        /// Maps a queryable of entities to a queryable of DTOs.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <param name="searchQuery">Queryable of entities.</param>
        /// <returns>Queryable of DTOs.</returns>
        public IQueryable<TDto> MapEntityToDto<TDto>(IQueryable<TEntity> searchQuery)
        {
            return _mapper.ProjectTo<TDto>(searchQuery);
            // return searchQuery.ProjectTo<TDto>(_mapper.ConfigurationProvider);
        }

        /// <summary>
        /// Validates a DTO using a validator.
        /// </summary>
        /// <typeparam name="TDto">DTO type.</typeparam>
        /// <typeparam name="TValidator">Validator type.</typeparam>
        /// <param name="crudAction">CRUD action performed.</param>
        /// <param name="dto">DTO instance.</param>
        public void ValidateDto<TDto, TValidator>(CrudAction crudAction, TDto dto)
            where TDto : IEntity
            where TValidator : AbstractValidator<TDto>
        {
            _validationService.ValidateDto<TDto, TValidator>(dto, crudAction);
        }

        /// <summary>
        /// Applies filters to the queryable.
        /// </summary>
        /// <param name="pagination">Pagination settings.</param>
        /// <param name="query">Queryable of entities.</param>
        /// <returns>Paginated IQueryable</returns>
        protected static IQueryable<TEntity> ApplyPagination(IPagination pagination, IQueryable<TEntity> query)
        {
            if (pagination != null && pagination.IsValid() && !pagination.Disabled)
            {
                query = query
                    .Skip((pagination.Page.Value - 1) * pagination.PageSize.Value)
                    .Take(pagination.PageSize.Value);
            }

            return query;
        }
    }
}
