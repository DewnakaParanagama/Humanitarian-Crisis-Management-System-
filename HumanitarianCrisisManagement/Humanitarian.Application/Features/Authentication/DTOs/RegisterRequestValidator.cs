using FluentValidation;

namespace Humanitarian.Application.Features.Authentication.DTOs;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
        .NotEmpty()
        .MaximumLength(150);


    RuleFor(x => x.Email)
        .NotEmpty()
        .EmailAddress()
        .MaximumLength(256);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .Must(role =>
                role == "Citizen" || role == "Volunteer")
            .WithMessage("Public registration allows only Citizen or Volunteer roles.");
    }


}
