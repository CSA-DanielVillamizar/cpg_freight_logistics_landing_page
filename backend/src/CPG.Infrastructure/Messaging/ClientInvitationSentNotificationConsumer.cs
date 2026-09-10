using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="ClientInvitationSentIntegrationEvent"/> and queues the invitation email
/// (T-SDD Epica 4). Recorded as an audit entry until a real email channel is wired up.
/// </summary>
public sealed class ClientInvitationSentNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<ClientInvitationSentNotificationConsumer> logger)
    : IConsumer<ClientInvitationSentIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ClientInvitationSentIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Client invitation queued for {Email} from agent {AgentId}", message.InvitedEmail, message.AgentId);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "ClientInvitationSent",
            EntityName = nameof(AgentClientInvitation),
            EntityId = message.InvitationId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new { message.AgentId, message.InvitedEmail }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
