namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a carrier accepts an available load and the load transitions to
/// <c>Dispatched</c>, so downstream systems (dispatch desk, shipper notifications) can react
/// asynchronously.
/// </summary>
public sealed record LoadAcceptedIntegrationEvent : IntegrationEvent
{
    public required Guid LoadId { get; init; }

    public required string Reference { get; init; }

    public required Guid CarrierId { get; init; }
}
