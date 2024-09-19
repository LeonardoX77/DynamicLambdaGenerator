using FluentValidation;
using Common.Core.CustomExceptions;

namespace Common.WebApi.Application.Models.Location
{
    public class LocationDtoValidator : AbstractValidator<LocationDto>
    {
        public LocationDtoValidator()
        {
            RuleFor(dto => dto.Name)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}")
                .MaximumLength(100)
                    .WithErrorCode($"{(int)ApiErrorCode.STRING_MAX_LENGTH}");

            RuleFor(dto => dto.Address)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}")
                .MaximumLength(200)
                    .WithErrorCode($"{(int)ApiErrorCode.STRING_MAX_LENGTH}");
        }
    }
}
