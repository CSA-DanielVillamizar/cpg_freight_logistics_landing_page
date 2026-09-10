namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a verified ELD webhook updates a load's last-known GPS position (T-SDD
/// Epica 2A). Consumed by <c>LoadGpsLocationUpdatedConsumer</c>, which forwards it to the
/// Shipper's live map over the <c>load-{loadId}</c> SignalR group.
/// </summary>
public sealed record LoadGpsLocationUpdatedIntegrationEvent : IntegrationEvent
{
    public required Guid LoadId { get; init; }

    public required string Reference { get; init; }

    public Guid? ShipperUserId { get; init; }

    public required decimal Latitude { get; init; }

    public required decimal Longitude { get; init; }

    public decimal? SpeedMph { get; init; }

    public required DateTimeOffset RecordedAtUtc { get; init; }
}
