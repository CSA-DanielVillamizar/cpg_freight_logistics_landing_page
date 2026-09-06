using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Admin.GetLoads;

/// <summary>
/// Every load for the admin control tower — <b>including</b> soft-deleted rows and synthetic
/// E2E fixtures, which the global query filter hides from all other reads. Newest first,
/// capped. Architectural exception: this is the one place loads are read with
/// <c>IgnoreQueryFilters()</c>.
/// </summary>
public sealed record GetAllLoadsQuery : IRequest<IReadOnlyList<AdminLoadView>>;

public sealed record AdminLoadView
{
    public required Guid Id { get; init; }

    public required string Reference { get; init; }

    public required LoadStatus Status { get; init; }

    public required bool IsDeleted { get; init; }

    public required bool IsSynthetic { get; init; }

    public required decimal RateUsd { get; init; }

    public required string Origin { get; init; }

    public required string Destination { get; init; }

    public Guid? ShipperUserId { get; init; }

    public string? CarrierName { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed class GetAllLoadsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAllLoadsQuery, IReadOnlyList<AdminLoadView>>
{
    private const int MaxRows = 200;

    public async Task<IReadOnlyList<AdminLoadView>> Handle(
        GetAllLoadsQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.Loads
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(load => load.AssignedCarrier)
            .OrderByDescending(load => load.CreatedAtUtc)
            .Take(MaxRows)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(load => new AdminLoadView
            {
                Id = load.Id,
                Reference = load.Reference,
                Status = load.Status,
                IsDeleted = load.IsDeleted,
                IsSynthetic = load.Reference.StartsWith("CPG-E2E-", StringComparison.Ordinal),
                RateUsd = load.RateUsd,
                Origin = $"{load.OriginCity}, {load.OriginState}",
                Destination = $"{load.DestinationCity}, {load.DestinationState}",
                ShipperUserId = load.ShipperUserId,
                CarrierName = load.AssignedCarrier?.CompanyName,
                CreatedAtUtc = load.CreatedAtUtc,
            })
            .ToList();
    }
}
