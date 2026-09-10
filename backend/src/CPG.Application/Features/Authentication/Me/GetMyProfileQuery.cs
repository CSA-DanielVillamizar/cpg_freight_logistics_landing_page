using MediatR;

namespace CPG.Application.Features.Authentication.Me;

/// <summary>GET /api/me — the authenticated principal plus their role-specific profile (T-SDD Epica 1).</summary>
public sealed record GetMyProfileQuery : IRequest<MyProfileResponse>;
