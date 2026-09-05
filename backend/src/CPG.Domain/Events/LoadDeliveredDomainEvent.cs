using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>
/// Raised when a load reaches <see cref="Enums.LoadStatus.Delivered"/>. Dispatched after the
/// PostgreSQL transaction commits; a handler forwards it to the broker, where the billing
/// consumer raises the shipper invoice.
/// </summary>
public sealed record LoadDeliveredDomainEvent(Guid LoadId, string Reference, Guid? ShipperUserId)
    : DomainEvent;
