using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="LoadAcceptedIntegrationEvent"/> from the broker and notifies the
/// dispatch desk. The notification is recorded as an audit entry so the end-to-end
/// publish -&gt; broker -&gt; consume path is observable.
/// </summary>
public sealed class LoadAcceptedNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<LoadAcceptedNotificationConsumer> logger)
    : IConsumer<LoadAcceptedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LoadAcceptedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Dispatch desk notified: load {Reference} ({LoadId}) accepted by carrier {CarrierId}",
            message.Reference,
            message.LoadId,
            message.CarrierId);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "DispatchDeskNotified",
            EntityName = nameof(Load),
            EntityId = message.LoadId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                message.Reference,
                message.CarrierId,
                message.EventId,
            }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
