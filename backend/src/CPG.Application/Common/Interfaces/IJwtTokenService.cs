using CPG.Domain.Entities;

namespace CPG.Application.Common.Interfaces;

/// <summary>Issued access + refresh token pair (SPEC.md US-01).</summary>
/// <param name="AccessToken">Signed JWT bearer token.</param>
/// <param name="ExpiresAtUtc">Absolute expiry of the access token.</param>
/// <param name="RefreshToken">Opaque refresh token string.</param>
/// <param name="RefreshTokenExpiresAtUtc">Absolute expiry of the refresh token.</param>
public readonly record struct TokenPair(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);

/// <summary>Creates signed JWT access tokens and companion refresh tokens.</summary>
public interface IJwtTokenService
{
    TokenPair IssueTokens(User user);

    string GenerateRefreshToken();
}
