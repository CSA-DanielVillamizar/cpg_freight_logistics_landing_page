using MediatR;

namespace CPG.Application.Features.Authentication.Login;

/// <summary>Exchanges email + password for a JWT access token and a refresh token (SPEC.md US-01).</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
