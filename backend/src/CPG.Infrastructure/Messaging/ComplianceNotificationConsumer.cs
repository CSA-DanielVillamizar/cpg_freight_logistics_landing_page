using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="ComplianceDocumentUploadedIntegrationEvent"/> from RabbitMQ and
/// notifies the commercial/compliance team (SPEC.md US-03). The notification is recorded as
/// an audit entry so the end-to-end publish -&gt; broker -&gt; consume path is observable.
/// </summary>
public sealed class ComplianceNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<ComplianceNotificationConsumer> logger)
    : IConsumer<ComplianceDocumentUploadedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ComplianceDocumentUploadedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Compliance review queued for carrier {CarrierId}, document {DocumentId} ({DocumentType})",
            message.CarrierId,
            message.DocumentId,
            message.DocumentType);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "CommercialTeamNotified",
            EntityName = nameof(Carrier),
            EntityId = message.CarrierId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                message.DocumentId,
                message.DocumentType,
                message.EventId,
            }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
