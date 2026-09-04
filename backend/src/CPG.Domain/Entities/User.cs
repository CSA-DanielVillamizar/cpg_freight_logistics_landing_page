using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>An authenticated platform principal (SPEC.md US-01).</summary>
public class User : AggregateRoot, IAuditableEntity
{
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string FullName { get; set; }

    public required UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; } = [];

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
