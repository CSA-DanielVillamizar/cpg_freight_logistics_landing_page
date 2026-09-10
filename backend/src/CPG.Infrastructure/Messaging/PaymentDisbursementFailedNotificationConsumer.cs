using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="PaymentDisbursementFailedIntegrationEvent"/> and alerts Admin/Finance
/// (T-SDD Epica 2B). Recorded as an audit entry until a real alerting channel is wired up.
/// </summary>
public sealed class PaymentDisbursementFailedNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<PaymentDisbursementFailedNotificationConsumer> logger)
    : IConsumer<PaymentDisbursementFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PaymentDisbursementFailedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogWarning(
            "Payout failed for load {LoadId}, carrier {CarrierId}: {Reason}",
            message.LoadId,
            message.CarrierId,
            message.Reason);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "PayoutFailed",
            EntityName = nameof(Domain.Entities.PaymentDisbursement),
            EntityId = message.LoadId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new { message.CarrierId, message.Reason }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
