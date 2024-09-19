using FluentValidation;
using Common.Core.Data.Identity.Enums;
using Common.Core.Data.Interfaces;

namespace Common.Core.Generic.Services
{
    /// <summary>
    /// Validation Service for classes that implement AbstractValidator<T>
    /// </summary>
    public class ValidationService : IValidationService
    {
        public bool ValidateDto<TDto, TValidator>(TDto dto, CrudAction crudAction)
            where TDto : IEntity
            where TValidator : AbstractValidator<TDto>
        {
            var validator = (TValidator)Activator.CreateInstance(typeof(TValidator), [crudAction == CrudAction.UPDATE_PATCH]);
            validator.ValidateAndThrow(dto);
            return true;
        }

    }
}
