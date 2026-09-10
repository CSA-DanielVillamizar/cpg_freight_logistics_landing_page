namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a shipper invoice is persisted for a delivered load (T-SDD Epica 2B). Consumed
/// to pre-compute the Carrier's <c>PaymentDisbursement</c> split ahead of settlement.
/// </summary>
public sealed record InvoiceGeneratedIntegrationEvent : IntegrationEvent
{
    public required Guid InvoiceId { get; init; }

    public required Guid LoadId { get; init; }

    public required decimal AmountUsd { get; init; }
}
