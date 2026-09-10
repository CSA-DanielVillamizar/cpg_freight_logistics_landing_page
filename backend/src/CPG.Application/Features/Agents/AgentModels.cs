using CPG.Domain.Enums;

namespace CPG.Application.Features.Agents;

/// <summary>POST /api/agents/clients/invite request body (T-SDD Epica 4).</summary>
public sealed record InviteClientRequest
{
    public required string InvitedEmail { get; init; }
}

/// <summary>201 response for a newly sent client invitation.</summary>
public sealed record AgentClientInvitationResponse
{
    public required Guid InvitationId { get; init; }

    public required string InvitedEmail { get; init; }

    public required InvitationStatus Status { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }
}

/// <summary>A row of GET /api/agents/clients (T-SDD Epica 4).</summary>
public sealed record AgentClientView
{
    public required Guid InvitationId { get; init; }

    public required string InvitedEmail { get; init; }

    public required InvitationStatus Status { get; init; }

    public required DateTimeOffset InvitedAtUtc { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public Guid? AcceptedByUserId { get; init; }
}

/// <summary>A row in the Agent dashboard's recent-loads table.</summary>
public sealed record AgentLoadView
{
    public required Guid LoadId { get; init; }

    public required string Reference { get; init; }

    public required LoadStatus Status { get; init; }

    public required ServiceType ServiceType { get; init; }

    public required string OriginCity { get; init; }

    public required string OriginState { get; init; }

    public required string DestinationCity { get; init; }

    public required string DestinationState { get; init; }

    public required decimal RateUsd { get; init; }

    public decimal? ProjectedAgentCommissionUsd { get; init; }

    public required DateTimeOffset PickupAtUtc { get; init; }
}

/// <summary>GET /api/agents/{id}/dashboard response (T-SDD Epica 4).</summary>
public sealed record AgentDashboardResponse
{
    public required Guid AgentId { get; init; }

    public required string CompanyName { get; init; }

    public required AgentStatus Status { get; init; }

    public required decimal CommissionRatePercent { get; init; }

    public required int ActiveLoadsCount { get; init; }

    public required int DeliveredLoadsCount { get; init; }

    /// <summary>Sum of <c>ProjectedAgentCommissionUsd</c> across loads not yet settled.</summary>
    public required decimal ProjectedCommissionUsd { get; init; }

    /// <summary>Sum of commission actually paid out via completed disbursements.</summary>
    public required decimal AccruedCommissionUsd { get; init; }

    public required IReadOnlyList<AgentLoadView> RecentLoads { get; init; }
}
