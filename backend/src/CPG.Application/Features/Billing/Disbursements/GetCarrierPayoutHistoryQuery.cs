using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.Disbursements;

/// <summary>The authenticated Carrier's payout ledger, newest first (T-SDD Epica 2B).</summary>
public sealed record GetCarrierPayoutHistoryQuery : IRequest<IReadOnlyList<PayoutHistoryEntryResponse>>;

public sealed class GetCarrierPayoutHistoryQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetCarrierPayoutHistoryQuery, IReadOnlyList<PayoutHistoryEntryResponse>>
{
    public async Task<IReadOnlyList<PayoutHistoryEntryResponse>> Handle(
        GetCarrierPayoutHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("A carrier profile is required to view payouts.");

        var disbursements = await dbContext.PaymentDisbursements
            .AsNoTracking()
            .Where(d => d.CarrierId == carrier.Id)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (disbursements.Count == 0)
        {
            return [];
        }

        var loadIds = disbursements.Select(d => d.LoadId).Distinct().ToArray();
        var referencesByLoadId = await dbContext.Loads
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => loadIds.Contains(l.Id))
            .Select(l => new { l.Id, l.Reference })
            .ToDictionaryAsync(l => l.Id, l => l.Reference, cancellationToken)
            .ConfigureAwait(false);

        return disbursements
            .Select(d => new PayoutHistoryEntryResponse
            {
                DisbursementId = d.Id,
                InvoiceId = d.InvoiceId,
                LoadReference = referencesByLoadId.GetValueOrDefault(d.LoadId, "—"),
                GrossAmountUsd = d.GrossAmountUsd,
                CpgMarginAmountUsd = d.CpgMarginAmountUsd,
                AgentCommissionAmountUsd = d.AgentCommissionAmountUsd,
                QuickPayFeeAmountUsd = d.QuickPayFeeAmountUsd,
                CarrierNetAmountUsd = d.CarrierNetAmountUsd,
                QuickPayRequested = d.QuickPayRequested,
                Status = d.Status,
                FailureReason = d.FailureReason,
                ProcessedAtUtc = d.ProcessedAtUtc,
                CreatedAtUtc = d.CreatedAtUtc,
            })
            .ToList();
    }
}
