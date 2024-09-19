using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Common.Core.Data.Identity.Enums;
using Common.Core.Data.Interfaces;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.Core.Generic.Controllers.Interfaces
{
    internal interface IBaseController<T, TKey>
        where T : class
        where TKey : struct
    {
        Task<IActionResult> Get<TDto>(TKey id);

        Task<IActionResult> Get<TDto, TQueryFilter>(TQueryFilter filter)
            where TQueryFilter : class, IDynamicQueryFilter, IPagination, new();

        Task<IActionResult> Update<TDto, TValidator>(CrudAction crudAction, TKey id, TDto dto)
            where TDto : class, IEntity
            where TValidator : AbstractValidator<TDto>;

        Task<IActionResult> Delete<TDto, TValidator>(TKey id)
            where TDto : class, IEntity
            where TValidator : AbstractValidator<TDto>;

        Task<IActionResult> Create<TResultDto, TRequestDto, TValidator>(TRequestDto dto)
            where TResultDto : class, IEntity
            where TRequestDto : class, IEntity
            where TValidator : AbstractValidator<TRequestDto>;
    }
}
