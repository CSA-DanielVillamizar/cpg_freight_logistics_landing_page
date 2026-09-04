using CPG.Domain.Enums;

namespace CPG.Application.Features.Authentication;

/// <summary>POST /api/auth/login request body (SPEC.md US-01).</summary>
public sealed record LoginRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}

/// <summary>POST /api/auth/refresh request body (SPEC.md US-01).</summary>
public sealed record RefreshRequest
{
    public required string RefreshToken { get; init; }
}

/// <summary>Authenticated principal summary returned alongside the tokens.</summary>
public sealed record AuthenticatedUser
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    public required UserRole Role { get; init; }
}

/// <summary>Login / refresh success response - access token, refresh token and user.</summary>
public sealed record AuthResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public required string RefreshToken { get; init; }

    public required AuthenticatedUser User { get; init; }
}
