using FluentValidation;

namespace CPG.Application.Features.Carriers.Register;

public sealed class RegisterCarrierCommandValidator : AbstractValidator<RegisterCarrierCommand>
{
    public RegisterCarrierCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .Length(2, 200)
            .Must(v => !v.Contains('<', StringComparison.Ordinal) && !v.Contains('>', StringComparison.Ordinal))
            .WithMessage("Company name contains disallowed characters.");

        RuleFor(x => x.DotNumber).MaximumLength(40).When(x => x.DotNumber is not null);
        RuleFor(x => x.McNumber).MaximumLength(40).When(x => x.McNumber is not null);
    }
}
