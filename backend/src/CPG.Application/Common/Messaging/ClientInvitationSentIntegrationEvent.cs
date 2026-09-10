namespace CPG.Application.Common.Messaging;

/// <summary>Raised when an Agent invites a prospective client to join CPG (T-SDD Epica 4).</summary>
public sealed record ClientInvitationSentIntegrationEvent : IntegrationEvent
{
    public required Guid InvitationId { get; init; }

    public required Guid AgentId { get; init; }

    public required string InvitedEmail { get; init; }

    public required string Token { get; init; }
}
