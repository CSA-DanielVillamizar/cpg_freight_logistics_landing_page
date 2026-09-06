using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Persistence;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Loads.Deliver;

public sealed class MarkLoadDeliveredCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<MarkLoadDeliveredCommand, LoadSummaryResponse>
{
    public async Task<LoadSummaryResponse> Handle(
        MarkLoadDeliveredCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("No carrier account is linked to the current user.");

        var load = await dbContext.Loads
            .OperableById()
            .Include(l => l.AssignedCarrier)
            .FirstOrDefaultAsync(l => l.Id == request.LoadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Load '{request.LoadId}' was not found.");

        if (load.AssignedCarrierId != carrier.Id)
        {
            throw new ForbiddenAccessException("This load is assigned to another carrier.");
        }

        // Domain guard: throws DomainException (-> 409) unless the load is Dispatched/InTransit.
        load.MarkDelivered();

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "LoadDelivered",
            EntityName = nameof(Load),
            EntityId = load.Id.ToString(),
            UserId = userId.ToString(),
            TimestampUtc = clock.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new { load.Reference, load.ShipperUserId }),
        });

        // SaveChanges commits; DispatchDomainEventsInterceptor then publishes the integration event.
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return LoadSummaryResponse.FromEntity(load);
    }
}
