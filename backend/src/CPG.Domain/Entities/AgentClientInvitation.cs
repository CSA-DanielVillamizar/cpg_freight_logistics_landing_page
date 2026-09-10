using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>A one-time-use invitation an Agent sends to onboard their own Shipper client (T-SDD Epica 4).</summary>
public class AgentClientInvitation : Entity, IAuditableEntity
{
    public required Guid AgentId { get; set; }

    public required string InvitedEmail { get; set; }

    public Guid? AcceptedByUserId { get; set; }

    /// <summary>Opaque, cryptographically random, single-use token.</summary>
    public required string Token { get; set; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Sent;

    public required DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
