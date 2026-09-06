using System.Text.RegularExpressions;
using FluentValidation;

namespace CPG.Application.Features.Loads.Create;

/// <summary>Input validation for posting a new load to the board.</summary>
public sealed partial class CreateLoadCommandValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadCommandValidator()
    {
        RuleFor(x => x.Reference)
            .MaximumLength(40)
            .Matches(ReferencePattern()).WithMessage("Reference must look like CPG-XXXXX.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reference));

        RuleFor(x => x.ServiceType).IsInEnum();
        RuleFor(x => x.EquipmentType).NotEmpty().MaximumLength(120);

        RuleFor(x => x.OriginCity).NotEmpty().MaximumLength(120);
        RuleFor(x => x.OriginState).NotEmpty().Length(2);
        RuleFor(x => x.OriginZip).NotEmpty().Matches(ZipPattern());
        RuleFor(x => x.DestinationCity).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DestinationState).NotEmpty().Length(2);
        RuleFor(x => x.DestinationZip).NotEmpty().Matches(ZipPattern());

        RuleFor(x => x.DistanceMiles).GreaterThan(0).LessThanOrEqualTo(6000);
        RuleFor(x => x.WeightLbs).GreaterThan(0).LessThanOrEqualTo(200_000);
        RuleFor(x => x.RateUsd).GreaterThan(0m).LessThanOrEqualTo(1_000_000m);

        RuleFor(x => x.ShipperName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.DeliveryAtUtc)
            .GreaterThan(x => x.PickupAtUtc)
            .WithMessage("Delivery must be after pickup.");

        RuleFor(x => x.TargetTemperatureF)
            .InclusiveBetween(-40, 120)
            .When(x => x.TargetTemperatureF is not null);

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(1000)
            .When(x => x.SpecialInstructions is not null);
    }

    [GeneratedRegex(@"^CPG-[A-Za-z0-9-]{2,30}$")]
    private static partial Regex ReferencePattern();

    [GeneratedRegex(@"^\d{5}$")]
    private static partial Regex ZipPattern();
}
