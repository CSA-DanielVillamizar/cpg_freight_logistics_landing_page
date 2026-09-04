using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Leads.Create;

/// <summary>Captures a qualified enterprise lead from a public vertical landing page (SPEC.md US-04).</summary>
public sealed record CreateLeadCommand(
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string Phone,
    string VerticalSlug,
    ServiceType? ServiceType,
    string CargoDetails) : IRequest<CreateLeadResponse>
{
    public static CreateLeadCommand FromRequest(CreateLeadRequest request) => new(
        request.CompanyName,
        request.ContactName,
        request.ContactEmail,
        request.Phone,
        request.VerticalSlug,
        request.ServiceType,
        request.CargoDetails);
}
