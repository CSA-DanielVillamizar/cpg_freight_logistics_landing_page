using CPG.Domain.Enums;

namespace CPG.Application.Features.Leads;

/// <summary>POST /api/leads request body (SPEC.md US-04).</summary>
public sealed record CreateLeadRequest
{
    public required string CompanyName { get; init; }

    public required string ContactName { get; init; }

    public required string ContactEmail { get; init; }

    public required string Phone { get; init; }

    /// <summary>Slug of the vertical landing page the inquiry came from.</summary>
    public required string VerticalSlug { get; init; }

    public ServiceType? ServiceType { get; init; }

    public required string CargoDetails { get; init; }
}

/// <summary>POST /api/leads 200 response.</summary>
public sealed record CreateLeadResponse
{
    public required Guid Id { get; init; }

    public required LeadStatus Status { get; init; }
}
