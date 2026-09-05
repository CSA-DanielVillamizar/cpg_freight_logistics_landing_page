using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Loads.GetLoads;

/// <summary>
/// Lists loads on the board, newest pickup first, with optional filters mirroring the
/// Load Workspace sidebar (status, service type, origin, destination). Empty filter
/// collections mean "no restriction".
/// </summary>
public sealed record GetLoadsQuery(
    IReadOnlyList<LoadStatus>? Statuses = null,
    IReadOnlyList<ServiceType>? ServiceTypes = null,
    string? Origin = null,
    string? Destination = null) : IRequest<IReadOnlyList<LoadSummaryResponse>>;

public sealed class GetLoadsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLoadsQuery, IReadOnlyList<LoadSummaryResponse>>
{
    private const int MaxRows = 200;

    public async Task<IReadOnlyList<LoadSummaryResponse>> Handle(
        GetLoadsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Loads
            .AsNoTracking()
            .Include(load => load.AssignedCarrier)
            .AsQueryable();

        if (request.Statuses is { Count: > 0 } statuses)
        {
            query = query.Where(load => statuses.Contains(load.Status));
        }

        if (request.ServiceTypes is { Count: > 0 } serviceTypes)
        {
            query = query.Where(load => serviceTypes.Contains(load.ServiceType));
        }

        var loads = await query
            .OrderByDescending(load => load.PickupAtUtc)
            .Take(MaxRows)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Free-text lane filters run in memory: the board is small (capped at MaxRows) and
        // this keeps the query provider-agnostic (no ILike / collation concerns).
        IEnumerable<Domain.Entities.Load> filtered = loads;

        if (!string.IsNullOrWhiteSpace(request.Origin))
        {
            var origin = request.Origin.Trim();
            filtered = filtered.Where(load =>
                Contains(load.OriginCity, origin) ||
                Contains(load.OriginState, origin) ||
                Contains(load.OriginZip, origin));
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            var destination = request.Destination.Trim();
            filtered = filtered.Where(load =>
                Contains(load.DestinationCity, destination) ||
                Contains(load.DestinationState, destination) ||
                Contains(load.DestinationZip, destination));
        }

        return filtered.Select(LoadSummaryResponse.FromEntity).ToList();
    }

    private static bool Contains(string value, string term)
        => value.Contains(term, StringComparison.OrdinalIgnoreCase);
}
