namespace CPG.Application.Common.Messaging;

/// <summary>Raised when a Carrier's payout cannot be transferred via Stripe Connect (T-SDD Epica 2B).</summary>
public sealed record PaymentDisbursementFailedIntegrationEvent : IntegrationEvent
{
    public required Guid LoadId { get; init; }

    public required Guid CarrierId { get; init; }

    public required string Reason { get; init; }
}
