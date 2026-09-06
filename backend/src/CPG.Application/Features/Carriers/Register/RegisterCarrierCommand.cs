using MediatR;

namespace CPG.Application.Features.Carriers.Register;

/// <summary>
/// A carrier user creates their carrier profile so they can accept loads and file compliance
/// documents. One profile per user; the account starts <c>PendingCompliance</c>.
/// </summary>
public sealed record RegisterCarrierCommand(
    string CompanyName,
    string? DotNumber,
    string? McNumber) : IRequest<CarrierRegistrationResponse>
{
    public static RegisterCarrierCommand FromRequest(RegisterCarrierRequest request) =>
        new(request.CompanyName, request.DotNumber, request.McNumber);
}
