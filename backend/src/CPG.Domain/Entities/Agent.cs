using CPG.Domain.Common;
using CPG.Domain.Enums;
using CPG.Domain.Events;

namespace CPG.Domain.Entities;

/// <summary>An independent freight agent operating under CPG's DOT/MC license (T-SDD Epica 1 / Epica 4).</summary>
public class Agent : AggregateRoot, IAuditableEntity, IHasRowVersion, IHasStripeConnectAccount
{
    public required string CompanyName { get; set; }

    public required Guid UserId { get; set; }

    /// <summary>Reference to the CPG DOT/MC license the agent operates under.</summary>
    public required string CpgLicenseReference { get; set; }

    public decimal CommissionRatePercent { get; set; } = 10.0m;

    public AgentStatus Status { get; private set; } = AgentStatus.PendingActivation;

    public string? StripeConnectAccountId { get; set; }

    /// <summary>Optimistic concurrency token mapped to PostgreSQL <c>xmin</c>.</summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>An administrator activates the agent once their agency checklist is complete.</summary>
    /// <exception cref="DomainException">The agent is not pending activation.</exception>
    public void Activate()
    {
        if (Status != AgentStatus.PendingActivation)
        {
            throw new DomainException($"Agent {Id} cannot be activated from status {Status}.");
        }

        Status = AgentStatus.Active;

        RaiseDomainEvent(new AgentActivatedDomainEvent(Id, UserId));
    }
}
