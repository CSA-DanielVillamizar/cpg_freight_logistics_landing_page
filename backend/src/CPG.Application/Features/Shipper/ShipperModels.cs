using CPG.Domain.Enums;

namespace CPG.Application.Features.Shipper;

/// <summary>A single load as seen by the corporate shipper that requested it.</summary>
public sealed record ShipperLoadView
{
    public required Guid Id { get; init; }

    public required string Reference { get; init; }

    public required LoadStatus Status { get; init; }

    public required ServiceType ServiceType { get; init; }

    public required string EquipmentType { get; init; }

    public required string OriginCity { get; init; }

    public required string OriginState { get; init; }

    public required string DestinationCity { get; init; }

    public required string DestinationState { get; init; }

    public required int DistanceMiles { get; init; }

    public required int WeightLbs { get; init; }

    public required decimal RateUsd { get; init; }

    public required DateTimeOffset PickupAtUtc { get; init; }

    public required DateTimeOffset DeliveryAtUtc { get; init; }

    public string? CarrierName { get; init; }

    /// <summary>True when a proof-of-delivery document is available for download.</summary>
    public required bool PodAvailable { get; init; }
}

/// <summary>The shipper dashboard payload: active shipments, delivered history and headline metrics.</summary>
public sealed record ShipperLoadsResponse
{
    public required IReadOnlyList<ShipperLoadView> Active { get; init; }

    public required IReadOnlyList<ShipperLoadView> History { get; init; }

    public required ShipperLoadMetrics Metrics { get; init; }
}

public sealed record ShipperLoadMetrics
{
    public required int ActiveCount { get; init; }

    public required int InTransitCount { get; init; }

    public required int DeliveredCount { get; init; }

    public required decimal ActiveSpendUsd { get; init; }
}

/// <summary>The bytes of a proof-of-delivery document, streamed to the owning shipper.</summary>
public sealed record LoadPodContent
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required Stream Content { get; init; }
}
