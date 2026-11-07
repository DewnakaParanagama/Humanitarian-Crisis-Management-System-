using FluentValidation;

namespace Humanitarian.Application.Features.Authentication.DTOs;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
        .NotEmpty()
        .EmailAddress()
        .MaximumLength(256);


    RuleFor(x => x.Password)
        .NotEmpty()
        .MaximumLength(100);
    }

}
