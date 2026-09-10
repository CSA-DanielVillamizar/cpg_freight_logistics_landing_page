using CPG.Domain.Common;
using CPG.Domain.Enums;
using CPG.Domain.Events;

namespace CPG.Domain.Entities;

/// <summary>An authenticated platform principal (SPEC.md US-01).</summary>
public class User : AggregateRoot, IAuditableEntity
{
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string FullName { get; set; }

    public required UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Company name for Shipper/Agent principals without a dedicated aggregate.</summary>
    public string? CompanyName { get; set; }

    public string? PhoneNumber { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; } = [];

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Self-registers a new principal via the Tri-Sign-Up flow (T-SDD Epica 1). Admin
    /// accounts cannot be created through this path.
    /// </summary>
    /// <exception cref="DomainException">The requested role is <see cref="UserRole.Admin"/>.</exception>
    public static User Register(
        string email,
        string passwordHash,
        string fullName,
        UserRole role,
        string? companyName,
        string? phoneNumber)
    {
        if (role == UserRole.Admin)
        {
            throw new DomainException("Admin accounts cannot be self-registered.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = role,
            CompanyName = companyName,
            PhoneNumber = phoneNumber,
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email, user.Role));

        return user;
    }
}
