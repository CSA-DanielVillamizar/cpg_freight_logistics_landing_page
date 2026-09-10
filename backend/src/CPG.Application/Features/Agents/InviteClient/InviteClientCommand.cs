using MediatR;

namespace CPG.Application.Features.Agents.InviteClient;

/// <summary>
/// An Agent invites a prospective Shipper client to join CPG under their agency
/// (T-SDD Epica 4). The invitation token is single-use and expires in 7 days.
/// </summary>
public sealed record InviteClientCommand(string InvitedEmail) : IRequest<AgentClientInvitationResponse>;
