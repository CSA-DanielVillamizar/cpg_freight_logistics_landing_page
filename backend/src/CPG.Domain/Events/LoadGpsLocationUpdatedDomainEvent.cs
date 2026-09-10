using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>
/// Raised when a verified ELD webhook updates a load's last-known GPS position
/// (T-SDD Epica 2A). Forwarded to Azure Service Bus so the Shipper's live map updates in
/// real time via Azure SignalR Service.
/// </summary>
public sealed record LoadGpsLocationUpdatedDomainEvent(
    Guid LoadId,
    string Reference,
    decimal Latitude,
    decimal Longitude,
    decimal? SpeedMph,
    DateTimeOffset RecordedAtUtc) : DomainEvent;
