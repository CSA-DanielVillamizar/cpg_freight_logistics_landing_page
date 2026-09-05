namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a load transitions to <c>Delivered</c>. The billing consumer reacts by raising
/// a <c>Pending</c> shipper invoice for the load.
/// </summary>
public sealed record LoadDeliveredIntegrationEvent : IntegrationEvent
{
    public required Guid LoadId { get; init; }

    public required string Reference { get; init; }

    public Guid? ShipperUserId { get; init; }
}
