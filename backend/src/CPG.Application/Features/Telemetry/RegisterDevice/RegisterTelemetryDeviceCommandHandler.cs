using System.Security.Cryptography;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Telemetry.RegisterDevice;

public sealed class RegisterTelemetryDeviceCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<RegisterTelemetryDeviceCommand, TelemetryDeviceRegistrationResponse>
{
    public async Task<TelemetryDeviceRegistrationResponse> Handle(
        RegisterTelemetryDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("A carrier profile is required before registering a telemetry device.");

        var alreadyRegistered = await dbContext.TelemetryDevices
            .AnyAsync(
                d => d.Provider == request.Provider && d.ExternalDeviceId == request.ExternalDeviceId,
                cancellationToken)
            .ConfigureAwait(false);

        if (alreadyRegistered)
        {
            throw new DomainException(
                $"A '{request.Provider}' device with id '{request.ExternalDeviceId}' is already registered.");
        }

        // The webhook needs the raw secret back to recompute the HMAC on every inbound call, so
        // it is stored as-is rather than one-way hashed (unlike a login password). Rotation /
        // envelope encryption via Key Vault is a tracked fast-follow, not a Sprint 2A blocker.
        var webhookSecret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        var device = new TelemetryDevice
        {
            CarrierId = carrier.Id,
            Provider = request.Provider,
            ExternalDeviceId = request.ExternalDeviceId,
            WebhookSecretHash = webhookSecret,
        };

        dbContext.TelemetryDevices.Add(device);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new TelemetryDeviceRegistrationResponse
        {
            DeviceId = device.Id,
            Provider = device.Provider,
            ExternalDeviceId = device.ExternalDeviceId,
            WebhookSecret = webhookSecret,
            WebhookUrl = $"/api/webhooks/telemetry/{device.Provider.ToString().ToLowerInvariant()}",
        };
    }
}
