using System.Text.RegularExpressions;
using FluentValidation;

namespace CPG.Application.Features.Leads.Create;

/// <summary>
/// Strict input validation for the public lead endpoint - guards against spam and injection
/// attempts (SPEC.md US-04: "validate all mandatory fields successfully").
/// </summary>
public sealed partial class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .Length(2, 200)
            .Must(NoMarkup).WithMessage("Company name contains disallowed characters.");

        RuleFor(x => x.ContactName)
            .NotEmpty()
            .Length(2, 200)
            .Must(NoMarkup).WithMessage("Contact name contains disallowed characters.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(40)
            .Matches(PhonePattern()).WithMessage("Phone number format is invalid.");

        RuleFor(x => x.VerticalSlug)
            .NotEmpty()
            .MaximumLength(120)
            .Matches(SlugPattern()).WithMessage("Vertical slug must be lowercase kebab-case.");

        RuleFor(x => x.ServiceType)
            .IsInEnum()
            .When(x => x.ServiceType is not null);

        RuleFor(x => x.CargoDetails)
            .NotEmpty()
            .Length(5, 2000)
            .Must(NoMarkup).WithMessage("Cargo details contain disallowed markup.")
            .Must(NoLinks).WithMessage("Links are not allowed in cargo details.");
    }

    private static bool NoMarkup(string value)
        => !value.Contains('<', StringComparison.Ordinal) && !value.Contains('>', StringComparison.Ordinal);

    private static bool NoLinks(string value)
        => !LinkPattern().IsMatch(value);

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex(@"^[+()\-.\s0-9]{7,40}$")]
    private static partial Regex PhonePattern();

    [GeneratedRegex(@"https?://|www\.", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex LinkPattern();
}
