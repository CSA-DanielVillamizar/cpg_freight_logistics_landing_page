using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Messaging;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using CPG.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="LoadDeliveredIntegrationEvent"/> from the broker and raises a
/// <c>Pending</c> shipper invoice for the load (net-30). Idempotent — one invoice per load.
/// </summary>
public sealed class LoadDeliveredNotificationConsumer(
    ApplicationDbContext dbContext,
    ILogger<LoadDeliveredNotificationConsumer> logger)
    : IConsumer<LoadDeliveredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LoadDeliveredIntegrationEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        if (message.ShipperUserId is null)
        {
            logger.LogInformation(
                "Load {Reference} delivered with no shipper on file — no invoice raised", message.Reference);
            return;
        }

        // System process: operate on the full dataset. A synthetic E2E load is hidden from
        // every user-facing query but its delivery must still raise (and de-dupe) an invoice.
        var alreadyBilled = await dbContext.Invoices
            .IgnoreQueryFilters()
            .AnyAsync(invoice => invoice.LoadId == message.LoadId, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyBilled)
        {
            return;
        }

        var load = await dbContext.Loads
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == message.LoadId, cancellationToken)
            .ConfigureAwait(false);

        if (load is null || load.IsDeleted || load.Status != LoadStatus.Delivered)
        {
            return;
        }

        var invoice = Invoice.ForDeliveredLoad(
            load,
            $"INV-{load.Reference.Replace("CPG-", string.Empty, StringComparison.Ordinal)}",
            DateTimeOffset.UtcNow);

        dbContext.Invoices.Add(invoice);
        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "InvoiceGenerated",
            EntityName = nameof(Invoice),
            EntityId = invoice.Id.ToString(),
            UserId = invoice.ShipperUserId.ToString(),
            TimestampUtc = DateTimeOffset.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                invoice.Reference,
                LoadReference = load.Reference,
                invoice.AmountUsd,
                message.EventId,
            }),
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Invoice {InvoiceReference} raised for delivered load {LoadReference} (${Amount})",
            invoice.Reference,
            load.Reference,
            invoice.AmountUsd);
    }
}
