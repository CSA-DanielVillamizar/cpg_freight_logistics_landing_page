using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Carriers.Register;

public sealed class RegisterCarrierCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<RegisterCarrierCommand, CarrierRegistrationResponse>
{
    public async Task<CarrierRegistrationResponse> Handle(
        RegisterCarrierCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var alreadyRegistered = await dbContext.Carriers
            .AnyAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyRegistered)
        {
            throw new DomainException("A carrier profile is already linked to this account.");
        }

        var carrier = new Carrier
        {
            CompanyName = request.CompanyName.Trim(),
            UserId = userId,
            DotNumber = string.IsNullOrWhiteSpace(request.DotNumber) ? null : request.DotNumber.Trim(),
            McNumber = string.IsNullOrWhiteSpace(request.McNumber) ? null : request.McNumber.Trim(),
        };

        dbContext.Carriers.Add(carrier);
        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "CarrierRegistered",
            EntityName = nameof(Carrier),
            EntityId = carrier.Id.ToString(),
            UserId = userId.ToString(),
            TimestampUtc = clock.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new { carrier.CompanyName, carrier.DotNumber, carrier.McNumber }),
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CarrierRegistrationResponse
        {
            CarrierId = carrier.Id,
            CompanyName = carrier.CompanyName,
            ComplianceStatus = carrier.ComplianceStatus,
        };
    }
}
