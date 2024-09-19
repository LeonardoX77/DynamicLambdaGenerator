using FluentValidation;
using Common.Core.Data.Identity.Enums;

namespace Common.Core.Data.Interfaces
{
    /// <summary>
    /// Validation Service for classes that implement AbstractValidator<T>
    /// </summary>
    public interface IValidationService
    {
        bool ValidateDto<TDto, TValidator>(TDto dto, CrudAction crudAction)
            where TDto : IEntity
            where TValidator : AbstractValidator<TDto>;
    }
}
