using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="AgentCommissionAccruedIntegrationEvent"/> and records the accrual as
/// audited (T-SDD Epica 4). A production build would also push a dashboard notification here.
/// </summary>
public sealed class AgentCommissionAccruedNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<AgentCommissionAccruedNotificationConsumer> logger)
    : IConsumer<AgentCommissionAccruedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AgentCommissionAccruedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Commission accrued for agent {AgentId} on load {LoadId}: ${Amount}",
            message.AgentId,
            message.LoadId,
            message.CommissionAmountUsd);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "AgentCommissionAccrued",
            EntityName = nameof(Domain.Entities.Agent),
            EntityId = message.AgentId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new { message.LoadId, message.CommissionAmountUsd }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
