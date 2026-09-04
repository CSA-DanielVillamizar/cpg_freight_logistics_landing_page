using CPG.Domain.Common;

namespace CPG.Domain.Entities;

/// <summary>Opaque refresh token issued alongside a JWT access token (SPEC.md US-01).</summary>
public class RefreshToken : Entity
{
    public required Guid UserId { get; set; }

    public required string Token { get; set; }

    public required DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTimeOffset.UtcNow < ExpiresAtUtc;
}
