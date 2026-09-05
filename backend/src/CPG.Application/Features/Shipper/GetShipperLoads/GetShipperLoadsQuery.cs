using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Shipper.GetShipperLoads;

/// <summary>
/// Loads requested by the authenticated corporate shipper (JWT subject), split into active
/// shipments (<c>Dispatched</c>, <c>InTransit</c>) and delivered history.
/// </summary>
public sealed record GetShipperLoadsQuery : IRequest<ShipperLoadsResponse>;

public sealed class GetShipperLoadsQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<GetShipperLoadsQuery, ShipperLoadsResponse>
{
    private static readonly LoadStatus[] Visible =
        [LoadStatus.Dispatched, LoadStatus.InTransit, LoadStatus.Delivered];

    public async Task<ShipperLoadsResponse> Handle(
        GetShipperLoadsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var loads = await dbContext.Loads
            .AsNoTracking()
            .Include(load => load.AssignedCarrier)
            .Where(load => load.ShipperUserId == userId && Visible.Contains(load.Status))
            .OrderByDescending(load => load.PickupAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var views = loads.Select(ToView).ToList();
        var active = views.Where(v => v.Status is LoadStatus.Dispatched or LoadStatus.InTransit).ToList();
        var history = views.Where(v => v.Status == LoadStatus.Delivered).ToList();

        return new ShipperLoadsResponse
        {
            Active = active,
            History = history,
            Metrics = new ShipperLoadMetrics
            {
                ActiveCount = active.Count,
                InTransitCount = active.Count(v => v.Status == LoadStatus.InTransit),
                DeliveredCount = history.Count,
                ActiveSpendUsd = active.Sum(v => v.RateUsd),
            },
        };
    }

    private static ShipperLoadView ToView(Load load) => new()
    {
        Id = load.Id,
        Reference = load.Reference,
        Status = load.Status,
        ServiceType = load.ServiceType,
        EquipmentType = load.EquipmentType,
        OriginCity = load.OriginCity,
        OriginState = load.OriginState,
        DestinationCity = load.DestinationCity,
        DestinationState = load.DestinationState,
        DistanceMiles = load.DistanceMiles,
        WeightLbs = load.WeightLbs,
        RateUsd = load.RateUsd,
        PickupAtUtc = load.PickupAtUtc,
        DeliveryAtUtc = load.DeliveryAtUtc,
        CarrierName = load.AssignedCarrier?.CompanyName,
        PodAvailable = load.Status == LoadStatus.Delivered && !string.IsNullOrWhiteSpace(load.PodBlobUri),
    };
}
