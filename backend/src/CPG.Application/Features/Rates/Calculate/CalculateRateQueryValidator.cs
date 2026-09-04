using CPG.Domain.Enums;
using FluentValidation;

namespace CPG.Application.Features.Rates.Calculate;

public sealed class CalculateRateQueryValidator : AbstractValidator<CalculateRateQuery>
{
    public CalculateRateQueryValidator()
    {
        RuleFor(x => x.ServiceType).IsInEnum();

        RuleFor(x => x.OriginZip)
            .NotEmpty()
            .Matches(@"^\d{5}$").WithMessage("Origin ZIP must be a 5-digit US ZIP code.");

        RuleFor(x => x.DestinationZip)
            .NotEmpty()
            .Matches(@"^\d{5}$").WithMessage("Destination ZIP must be a 5-digit US ZIP code.");

        RuleFor(x => x.WeightLbs)
            .InclusiveBetween(1, 200_000)
            .WithMessage("Weight must be between 1 and 200,000 lbs.");

        When(x => x.ServiceType == ServiceType.ColdChain, () =>
        {
            RuleFor(x => x.TargetTemperatureCelsius)
                .NotNull().WithMessage("Target temperature is required for Cold Chain shipments.")
                .InclusiveBetween(-60m, 30m);
        });
    }
}
