using MediatR;

namespace CPG.Application.Features.Authentication.Refresh;

/// <summary>Rotates a valid refresh token for a new access + refresh token pair (SPEC.md US-01).</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
