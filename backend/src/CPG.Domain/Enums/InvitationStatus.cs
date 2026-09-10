namespace CPG.Domain.Enums;

/// <summary>Lifecycle of an <see cref="Entities.AgentClientInvitation"/> (T-SDD Epica 4).</summary>
public enum InvitationStatus
{
    Sent = 1,
    Accepted = 2,
    Expired = 3,
    Revoked = 4,
}
