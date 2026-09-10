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

/// <summary>
/// POST /api/auth/register request body — the Tri-Sign-Up flow (T-SDD Epica 1). Every
/// visitor picks a business role and gets routed to the matching workspace.
/// </summary>
public sealed record RegisterRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string FullName { get; init; }

    public required UserRole Role { get; init; }

    public string? CompanyName { get; init; }

    public string? PhoneNumber { get; init; }

    /// <summary>Carrier-only: DOT number, collected up-front to seed the compliance profile.</summary>
    public string? DotNumber { get; init; }

    /// <summary>Carrier-only: MC number, collected up-front to seed the compliance profile.</summary>
    public string? McNumber { get; init; }
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

/// <summary>GET /api/me response — the authenticated principal plus their role-specific profile.</summary>
public sealed record MyProfileResponse
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    public required UserRole Role { get; init; }

    public string? CompanyName { get; init; }

    public string? PhoneNumber { get; init; }

    public CarrierProfileSummary? Carrier { get; init; }

    public AgentProfileSummary? Agent { get; init; }
}

public sealed record CarrierProfileSummary
{
    public required Guid CarrierId { get; init; }

    public required string CompanyName { get; init; }

    public required ComplianceStatus ComplianceStatus { get; init; }
}

public sealed record AgentProfileSummary
{
    public required Guid AgentId { get; init; }

    public required string CompanyName { get; init; }

    public required AgentStatus Status { get; init; }

    public required decimal CommissionRatePercent { get; init; }
}
