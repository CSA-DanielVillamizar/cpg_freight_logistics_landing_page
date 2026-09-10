using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Agents.GetDashboard;

/// <summary>
/// The Agent's commission dashboard: projected vs. accrued commission and recent loads
/// (T-SDD Epica 4). <paramref name="AgentId"/> must match the caller's own agent profile —
/// an Agent can never read another Agent's dashboard.
/// </summary>
public sealed record GetAgentDashboardQuery(Guid AgentId) : IRequest<AgentDashboardResponse>;

public sealed class GetAgentDashboardQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetAgentDashboardQuery, AgentDashboardResponse>
{
    private const int MaxRecentLoads = 20;

    public async Task<AgentDashboardResponse> Handle(
        GetAgentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var agent = await dbContext.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("An agent profile is required to view the dashboard.");

        if (agent.Id != request.AgentId)
        {
            throw new ForbiddenAccessException();
        }

        var loads = await dbContext.Loads
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.AgentId == agent.Id && !l.IsDeleted)
            .OrderByDescending(l => l.PickupAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var accruedCommissionUsd = await dbContext.PaymentDisbursements
            .AsNoTracking()
            .Where(d => d.AgentId == agent.Id && d.Status == DisbursementStatus.Completed)
            .SumAsync(d => (decimal?)d.AgentCommissionAmountUsd, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var projectedCommissionUsd = loads
            .Where(l => l.Status != LoadStatus.Delivered)
            .Sum(l => l.ProjectedAgentCommissionUsd ?? 0m);

        return new AgentDashboardResponse
        {
            AgentId = agent.Id,
            CompanyName = agent.CompanyName,
            Status = agent.Status,
            CommissionRatePercent = agent.CommissionRatePercent,
            ActiveLoadsCount = loads.Count(l => l.Status != LoadStatus.Delivered),
            DeliveredLoadsCount = loads.Count(l => l.Status == LoadStatus.Delivered),
            ProjectedCommissionUsd = projectedCommissionUsd,
            AccruedCommissionUsd = accruedCommissionUsd,
            RecentLoads = loads
                .Take(MaxRecentLoads)
                .Select(l => new AgentLoadView
                {
                    LoadId = l.Id,
                    Reference = l.Reference,
                    Status = l.Status,
                    ServiceType = l.ServiceType,
                    OriginCity = l.OriginCity,
                    OriginState = l.OriginState,
                    DestinationCity = l.DestinationCity,
                    DestinationState = l.DestinationState,
                    RateUsd = l.RateUsd,
                    ProjectedAgentCommissionUsd = l.ProjectedAgentCommissionUsd,
                    PickupAtUtc = l.PickupAtUtc,
                })
                .ToList(),
        };
    }
}
