using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.MarkInvoicePaid;

/// <summary>
/// Settles an invoice once Stripe confirms the checkout completed (webhook, or the mock
/// checkout page). Idempotent — a re-delivered event is a no-op. Not user-scoped.
/// </summary>
public sealed record MarkInvoicePaidCommand(string StripeSessionId) : IRequest<bool>;

public sealed class MarkInvoicePaidCommandHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider clock)
    : IRequestHandler<MarkInvoicePaidCommand, bool>
{
    public async Task<bool> Handle(MarkInvoicePaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.StripeSessionId == request.StripeSessionId, cancellationToken)
            .ConfigureAwait(false);

        if (invoice is null || invoice.Status == Domain.Enums.InvoiceStatus.Paid)
        {
            return false;
        }

        invoice.MarkPaid(clock.UtcNow);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "InvoicePaid",
            EntityName = nameof(Invoice),
            EntityId = invoice.Id.ToString(),
            UserId = invoice.ShipperUserId.ToString(),
            TimestampUtc = clock.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                invoice.Reference,
                invoice.AmountUsd,
                request.StripeSessionId,
            }),
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
