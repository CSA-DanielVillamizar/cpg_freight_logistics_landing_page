using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="QuickPayProcessedIntegrationEvent"/> and records the payout as audited
/// (T-SDD Epica 2B). A production build would also queue the Carrier's payout-confirmation email/SMS here.
/// </summary>
public sealed class QuickPayProcessedNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<QuickPayProcessedNotificationConsumer> logger)
    : IConsumer<QuickPayProcessedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<QuickPayProcessedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Payout settled for load {LoadId}: carrier {CarrierId} net ${Net} (Quick Pay: {QuickPay})",
            message.LoadId,
            message.CarrierId,
            message.CarrierNetAmountUsd,
            message.QuickPayRequested);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "PayoutSettled",
            EntityName = nameof(Domain.Entities.PaymentDisbursement),
            EntityId = message.LoadId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                message.CarrierId,
                message.CarrierNetAmountUsd,
                message.QuickPayRequested,
                message.StripeTransferId,
            }),
        });

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
