using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Common.Core.Data.Identity.Enums;
using Common.Core.Data.Interfaces;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Common.Core.Generic.Controllers.Response;
using Microsoft.Extensions.Logging;

namespace Common.Core.Generic.Controllers
{
    /// <summary>
    /// Controller base which inherits from BaseController and provides additional features:
    /// - Marks the controller as an API controller with [ApiController] attribute.
    /// - Requires authorization with [Authorize] attribute.
    /// - Handles CRUD operations using HTTP verbs.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TRequestDto">The DTO request type.</typeparam>
    /// <typeparam name="TResponseDto">The DTO response type.</typeparam>
    /// <typeparam name="TQueryFilter">The query filter type.</typeparam>
    /// <typeparam name="TValidator">The validator type.</typeparam>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class GenericControllerBase<TEntity, TRequestDto, TResponseDto, TQueryFilter, TValidator> : CustomBaseController<TEntity, int>
        where TEntity : class, IEntity
        where TRequestDto : class, IEntity
        where TResponseDto : class, IEntity
        where TQueryFilter : class, IDynamicQueryFilter, IPagination, new()
        where TValidator : AbstractValidator<TRequestDto>, new()
    {
        public GenericControllerBase(
            ILogger<CustomBaseController<TEntity, int>> logger,
            IBaseService<TEntity, int> service) : base(logger, service) { }

        /// <summary>
        /// Gets an entity by id.
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id}", Name = $"[controller]/{nameof(Get)}")]
        [ProducesGenericResponseType(typeof(Response<>), nameof(TResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
        public virtual async Task<IActionResult> Get(int id)
        {
            return await base.Get<TResponseDto>(id);
        }

        /// <summary>
        /// Gets a list of entities by filter.
        /// </summary>
        /// <param name="filter"></param>
        [HttpPost("query")]
        [ProducesGenericResponseType(typeof(Response<>), nameof(TResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public virtual async Task<IActionResult> Query(TQueryFilter filter)
        {
            return await base.Get<TResponseDto, TQueryFilter>(filter);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="dto"></param>
        [HttpPost]
        [ProducesResponseType(typeof(CreatedAtRoute), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public virtual async Task<IActionResult> Create([FromBody] TRequestDto dto)
        {
            return await base.Create<TResponseDto, TRequestDto, TValidator>(dto);
        }

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="dto"></param>
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public virtual async Task<IActionResult> Update([FromBody] TRequestDto dto)
        {
            return await base.Update<TRequestDto, TValidator>(CrudAction.UPDATE, dto.Id, dto);
        }

        /// <summary>
        /// Partially updates an entity.
        /// </summary>
        /// <param name="dto"></param>
        [HttpPatch]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public virtual async Task<IActionResult> Patch([FromBody] TRequestDto dto)
        {
            return await base.Update<TRequestDto, TValidator>(CrudAction.UPDATE_PATCH, dto.Id, dto);
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            return await base.Delete<TRequestDto, TValidator>(id);
        }
    }
}
