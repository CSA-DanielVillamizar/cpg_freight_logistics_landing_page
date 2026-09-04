using CPG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CPG.Api.Infrastructure;

/// <summary>RBAC policy names and registration (SPEC.md US-01).</summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CarrierOnly = "CarrierOnly";
    public const string ShipperOnly = "ShipperOnly";

    public static void AddCpgAuthorization(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(AdminOnly, p => p.RequireRole(nameof(UserRole.Admin)));
        builder.AddPolicy(CarrierOnly, p => p.RequireRole(nameof(UserRole.Carrier)));
        builder.AddPolicy(ShipperOnly, p => p.RequireRole(nameof(UserRole.Shipper)));
    }
}
