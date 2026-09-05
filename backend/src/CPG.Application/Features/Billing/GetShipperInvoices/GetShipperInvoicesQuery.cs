using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.GetShipperInvoices;

/// <summary>Invoices billed to the authenticated shipper, newest first, plus the balance owed.</summary>
public sealed record GetShipperInvoicesQuery : IRequest<ShipperInvoicesResponse>;

public sealed class GetShipperInvoicesQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<GetShipperInvoicesQuery, ShipperInvoicesResponse>
{
    public async Task<ShipperInvoicesResponse> Handle(
        GetShipperInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var now = clock.UtcNow;

        var rows = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.ShipperUserId == userId)
            .Join(
                dbContext.Loads.AsNoTracking(),
                invoice => invoice.LoadId,
                load => load.Id,
                (invoice, load) => new { invoice, load.Reference })
            .OrderByDescending(row => row.invoice.IssuedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var views = rows.Select(row =>
        {
            var raw = row.invoice.Status;
            var effective = raw == InvoiceStatus.Pending && row.invoice.DueDate < now
                ? InvoiceStatus.Overdue
                : raw;

            return new ShipperInvoiceView
            {
                Id = row.invoice.Id,
                Reference = row.invoice.Reference,
                LoadReference = row.Reference,
                AmountUsd = row.invoice.AmountUsd,
                Status = effective,
                IssuedAtUtc = row.invoice.IssuedAtUtc,
                DueDate = row.invoice.DueDate,
                PaidAtUtc = row.invoice.PaidAtUtc,
                Payable = effective is InvoiceStatus.Pending or InvoiceStatus.Overdue,
            };
        }).ToList();

        return new ShipperInvoicesResponse
        {
            Invoices = views,
            TotalOutstandingUsd = views.Where(v => v.Payable).Sum(v => v.AmountUsd),
            OverdueCount = views.Count(v => v.Status == InvoiceStatus.Overdue),
        };
    }
}
