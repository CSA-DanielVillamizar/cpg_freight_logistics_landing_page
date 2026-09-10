using FluentValidation;
using CPG.Domain.Enums;

namespace CPG.Application.Features.Authentication.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        // OWASP ASVS 2.1 baseline: 12+ chars, at least one of each character class.
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(256)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(role => role != UserRole.Admin)
            .WithMessage("Admin accounts cannot be self-registered.");

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required for Carrier and Agent accounts.")
            .When(x => x.Role is UserRole.Carrier or UserRole.Agent);

        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.DotNumber).MaximumLength(32);
        RuleFor(x => x.McNumber).MaximumLength(32);
    }
}
