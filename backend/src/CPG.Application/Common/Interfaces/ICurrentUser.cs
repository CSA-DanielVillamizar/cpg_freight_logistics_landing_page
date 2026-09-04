using CPG.Domain.Enums;

namespace CPG.Application.Common.Interfaces;

/// <summary>Ambient information about the authenticated principal for the current request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    UserRole? Role { get; }

    bool IsAuthenticated { get; }
}
