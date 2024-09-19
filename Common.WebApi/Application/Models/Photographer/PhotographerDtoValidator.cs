using FluentValidation;
using Common.WebApi.Application.Models.Client;
using Common.Core.CustomExceptions;

namespace Common.WebApi.Application.Models.Photographer
{
    public class PhotographerDtoValidator : AbstractValidator<PhotographerDto>
    {
        public PhotographerDtoValidator()
        {
            RuleFor(dto => dto.Name)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}")
                .MaximumLength(100)
                    .WithErrorCode($"{(int)ApiErrorCode.STRING_MAX_LENGTH}");

            RuleFor(dto => dto.Email)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}")
                .EmailAddress()
                    .WithErrorCode($"{(int)ApiErrorCode.INVALID_EMAIL_FORMAT}");

            RuleFor(dto => dto.PhoneNumber)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}")
                .MaximumLength(15)
                    .WithErrorCode($"{(int)ApiErrorCode.STRING_MAX_LENGTH}");
        }
    }
}
