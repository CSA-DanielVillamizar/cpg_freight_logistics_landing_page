using System.Security.Claims;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;

namespace CPG.Api.Infrastructure;

/// <summary>Resolves <see cref="ICurrentUser"/> from the ambient <see cref="HttpContext"/> principal.</summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Principal?.FindFirstValue("sub"), out var id)
            ? id
            : null;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email");

    public UserRole? Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
