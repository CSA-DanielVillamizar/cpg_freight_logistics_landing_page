using CPG.Domain.Entities;
using CPG.Domain.Enums;

namespace CPG.Application.Features.Loads;

/// <summary>
/// POST /api/loads request body. Posted by an Admin (on behalf of a shipper) or by a Shipper
/// booking their own freight. The load lands on the board as <c>Available</c>.
/// </summary>
public sealed record CreateLoadRequest
{
    /// <summary>Optional board reference (e.g. <c>CPG-48213</c>). Auto-generated when omitted.</summary>
    public string? Reference { get; init; }

    public required ServiceType ServiceType { get; init; }

    public required string EquipmentType { get; init; }

    public required string OriginCity { get; init; }

    public required string OriginState { get; init; }

    public required string OriginZip { get; init; }

    public required string DestinationCity { get; init; }

    public required string DestinationState { get; init; }

    public required string DestinationZip { get; init; }

    public required int DistanceMiles { get; init; }

    public required int WeightLbs { get; init; }

    public required decimal RateUsd { get; init; }

    public required string ShipperName { get; init; }

    /// <summary>
    /// The corporate shipper user this load bills to. Ignored when a Shipper posts (their own
    /// id is used); required from an Admin for the delivered-load invoice to be raised.
    /// </summary>
    public Guid? ShipperUserId { get; init; }

    public required DateTimeOffset PickupAtUtc { get; init; }

    public required DateTimeOffset DeliveryAtUtc { get; init; }

    public int? TargetTemperatureF { get; init; }

    public string? SpecialInstructions { get; init; }
}

/// <summary>A single load row for the Carrier &amp; Shipper Load Workspace.</summary>
public sealed record LoadSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Reference { get; init; }

    public required LoadStatus Status { get; init; }

    public required ServiceType ServiceType { get; init; }

    public required string EquipmentType { get; init; }

    public required string OriginCity { get; init; }

    public required string OriginState { get; init; }

    public required string OriginZip { get; init; }

    public required string DestinationCity { get; init; }

    public required string DestinationState { get; init; }

    public required string DestinationZip { get; init; }

    public required int DistanceMiles { get; init; }

    public required int WeightLbs { get; init; }

    public required decimal RateUsd { get; init; }

    public required string ShipperName { get; init; }

    public string? CarrierName { get; init; }

    public required DateTimeOffset PickupAtUtc { get; init; }

    public required DateTimeOffset DeliveryAtUtc { get; init; }

    public int? TargetTemperatureF { get; init; }

    public string? SpecialInstructions { get; init; }

    public static LoadSummaryResponse FromEntity(Load load) => new()
    {
        Id = load.Id,
        Reference = load.Reference,
        Status = load.Status,
        ServiceType = load.ServiceType,
        EquipmentType = load.EquipmentType,
        OriginCity = load.OriginCity,
        OriginState = load.OriginState,
        OriginZip = load.OriginZip,
        DestinationCity = load.DestinationCity,
        DestinationState = load.DestinationState,
        DestinationZip = load.DestinationZip,
        DistanceMiles = load.DistanceMiles,
        WeightLbs = load.WeightLbs,
        RateUsd = load.RateUsd,
        ShipperName = load.ShipperName,
        CarrierName = load.AssignedCarrier?.CompanyName,
        PickupAtUtc = load.PickupAtUtc,
        DeliveryAtUtc = load.DeliveryAtUtc,
        TargetTemperatureF = load.TargetTemperatureF,
        SpecialInstructions = load.SpecialInstructions,
    };
}
