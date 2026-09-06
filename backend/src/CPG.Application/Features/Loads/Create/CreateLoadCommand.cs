using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Loads.Create;

/// <summary>
/// Posts a new load to the Carrier &amp; Shipper Load Workspace. The load is created
/// <c>Available</c> and immediately shows on the board. Restricted to the Admin and Shipper
/// roles at the API boundary.
/// </summary>
public sealed record CreateLoadCommand(
    string? Reference,
    ServiceType ServiceType,
    string EquipmentType,
    string OriginCity,
    string OriginState,
    string OriginZip,
    string DestinationCity,
    string DestinationState,
    string DestinationZip,
    int DistanceMiles,
    int WeightLbs,
    decimal RateUsd,
    string ShipperName,
    Guid? ShipperUserId,
    DateTimeOffset PickupAtUtc,
    DateTimeOffset DeliveryAtUtc,
    int? TargetTemperatureF,
    string? SpecialInstructions) : IRequest<LoadSummaryResponse>
{
    public static CreateLoadCommand FromRequest(CreateLoadRequest request) => new(
        request.Reference,
        request.ServiceType,
        request.EquipmentType,
        request.OriginCity,
        request.OriginState,
        request.OriginZip,
        request.DestinationCity,
        request.DestinationState,
        request.DestinationZip,
        request.DistanceMiles,
        request.WeightLbs,
        request.RateUsd,
        request.ShipperName,
        request.ShipperUserId,
        request.PickupAtUtc,
        request.DeliveryAtUtc,
        request.TargetTemperatureF,
        request.SpecialInstructions);
}
