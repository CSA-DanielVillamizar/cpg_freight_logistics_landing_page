using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Loads.Delete;

public sealed class DeleteLoadCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<DeleteLoadCommand>
{
    public async Task Handle(DeleteLoadCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        // Admin audit path: bypass every query filter so soft-deleted and synthetic
        // E2E loads can still be addressed.
        var load = await dbContext.Loads
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == request.LoadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Load '{request.LoadId}' was not found.");

        if (load.IsDeleted)
        {
            return; // idempotent — already logically deleted
        }

        load.IsDeleted = true;

        var invoice = await dbContext.Invoices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.LoadId == load.Id, cancellationToken)
            .ConfigureAwait(false);

        var invoiceCancelled = false;
        if (invoice is not null && !invoice.IsDeleted)
        {
            // Throws DomainException (-> 409) if the invoice is already paid.
            invoice.Cancel();
            invoice.IsDeleted = true;
            invoiceCancelled = true;
        }

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "LoadDeleted",
            EntityName = nameof(Load),
            EntityId = load.Id.ToString(),
            UserId = userId.ToString(),
            TimestampUtc = clock.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                load.Reference,
                load.Status,
                InvoiceCancelled = invoiceCancelled,
                InvoiceReference = invoice?.Reference,
            }),
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
