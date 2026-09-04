using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="CorporateLeadGeneratedIntegrationEvent"/> from RabbitMQ and notifies
/// the commercial team (SPEC.md US-04). The notification is recorded as an audit entry so the
/// end-to-end publish -&gt; broker -&gt; consume path is observable.
/// </summary>
public sealed class LeadNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<LeadNotificationConsumer> logger)
    : IConsumer<CorporateLeadGeneratedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CorporateLeadGeneratedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "New corporate lead {LeadId} from {Company} on vertical {Vertical}",
            message.LeadId,
            message.CompanyName,
            message.VerticalSlug);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "CommercialTeamNotified",
            EntityName = nameof(Lead),
            EntityId = message.LeadId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                message.CompanyName,
                message.ContactEmail,
                message.VerticalSlug,
                message.EventId,
            }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
