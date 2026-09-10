using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Telemetry.RegisterDevice;

/// <summary>
/// A carrier links an ELD hardware unit so its webhook signature can be verified
/// (T-SDD Epica 2A). One profile per authenticated carrier account.
/// </summary>
public sealed record RegisterTelemetryDeviceCommand(TelemetryProvider Provider, string ExternalDeviceId)
    : IRequest<TelemetryDeviceRegistrationResponse>
{
    public static RegisterTelemetryDeviceCommand FromRequest(RegisterTelemetryDeviceRequest request) =>
        new(request.Provider, request.ExternalDeviceId);
}
