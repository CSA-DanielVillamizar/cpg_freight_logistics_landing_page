using CPG.Domain.Enums;

namespace CPG.Application.Features.Carriers;

/// <summary>POST /api/carriers request body — a carrier user creating their own profile.</summary>
public sealed record RegisterCarrierRequest
{
    public required string CompanyName { get; init; }

    public string? DotNumber { get; init; }

    public string? McNumber { get; init; }
}

/// <summary>POST /api/carriers 201 response.</summary>
public sealed record CarrierRegistrationResponse
{
    public required Guid CarrierId { get; init; }

    public required string CompanyName { get; init; }

    public required ComplianceStatus ComplianceStatus { get; init; }
}
