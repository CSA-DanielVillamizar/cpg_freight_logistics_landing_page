namespace CPG.Application.Common.Messaging;

/// <summary>Raised once a Carrier's payout settles via Stripe Connect (T-SDD Epica 2B).</summary>
public sealed record QuickPayProcessedIntegrationEvent : IntegrationEvent
{
    public required Guid LoadId { get; init; }

    public required Guid CarrierId { get; init; }

    public required decimal CarrierNetAmountUsd { get; init; }

    public required bool QuickPayRequested { get; init; }

    public required string StripeTransferId { get; init; }
}
