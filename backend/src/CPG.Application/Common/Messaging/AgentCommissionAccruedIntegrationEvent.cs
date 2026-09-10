namespace CPG.Application.Common.Messaging;

/// <summary>Raised when an Agent's commission on a load settles via Stripe Connect (T-SDD Epica 4).</summary>
public sealed record AgentCommissionAccruedIntegrationEvent : IntegrationEvent
{
    public required Guid AgentId { get; init; }

    public required Guid LoadId { get; init; }

    public required decimal CommissionAmountUsd { get; init; }
}
