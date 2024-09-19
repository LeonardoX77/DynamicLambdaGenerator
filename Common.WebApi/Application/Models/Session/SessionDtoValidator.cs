using FluentValidation;
using Common.WebApi.Application.Models.User;
using Common.WebApi.Application.Models.Client;
using Common.Core.CustomExceptions;

namespace Common.WebApi.Application.Models.Session
{
    public class SessionDtoValidator : AbstractValidator<SessionRequestDto>
    {
        public SessionDtoValidator()
        {
            RuleFor(dto => dto.Date)
                .GreaterThan(new DateTime(DateTime.Now.Year, 1, 1))
                    .WithErrorCode($"{(int)ApiErrorCode.OUT_OF_RANGE_DATE}")
                .LessThan(new DateTime(DateTime.Now.Year, 12, 31))
                    .WithErrorCode($"{(int)ApiErrorCode.OUT_OF_RANGE_DATE}");

            RuleFor(dto => dto.Time)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}");

            RuleFor(dto => dto.SessionType)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}")
                .MaximumLength(50)
                    .WithErrorCode($"{(int)ApiErrorCode.STRING_MAX_LENGTH}");

            RuleFor(dto => dto.Notes)
                .MaximumLength(500)
                    .WithErrorCode($"{(int)ApiErrorCode.STRING_MAX_LENGTH}");

            RuleFor(dto => dto.ClientId)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}");

            RuleFor(dto => dto.PhotographerId)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}");

            RuleFor(dto => dto.LocationId)
                .NotEmpty()
                    .WithErrorCode($"{(int)ApiErrorCode.REQUIRED_FIELD}");
        }
    }
}
