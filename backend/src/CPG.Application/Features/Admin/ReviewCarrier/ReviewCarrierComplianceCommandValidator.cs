using FluentValidation;

namespace CPG.Application.Features.Admin.ReviewCarrier;

public sealed class ReviewCarrierComplianceCommandValidator : AbstractValidator<ReviewCarrierComplianceCommand>
{
    public ReviewCarrierComplianceCommandValidator()
    {
        RuleFor(x => x.CarrierId).NotEmpty();
        RuleFor(x => x.Decision).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
