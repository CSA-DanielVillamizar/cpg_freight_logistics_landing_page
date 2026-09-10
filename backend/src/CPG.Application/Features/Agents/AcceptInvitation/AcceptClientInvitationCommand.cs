using MediatR;

namespace CPG.Application.Features.Agents.AcceptInvitation;

/// <summary>
/// The authenticated invitee accepts an Agent's client invitation, linking their account to
/// the Agent's roster (T-SDD Epica 4). If the invitee has no account yet, the frontend
/// registers them first (Tri-Sign-Up) and calls this immediately after, now authenticated.
/// </summary>
public sealed record AcceptClientInvitationCommand(string Token) : IRequest;
