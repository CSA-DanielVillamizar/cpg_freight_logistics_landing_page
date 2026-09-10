using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="UserRegisteredIntegrationEvent"/> from the broker and queues the
/// welcome notification (T-SDD Epica 1). Recorded as an audit entry so the end-to-end
/// publish -&gt; broker -&gt; consume path is observable, same as <see cref="LeadNotificationConsumer"/>.
/// </summary>
public sealed class UserWelcomeNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<UserWelcomeNotificationConsumer> logger)
    : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Welcome email queued for {Email} ({Role})",
            message.Email,
            message.Role);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "WelcomeEmailQueued",
            EntityName = nameof(User),
            EntityId = message.UserId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new { message.Email, message.Role }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
