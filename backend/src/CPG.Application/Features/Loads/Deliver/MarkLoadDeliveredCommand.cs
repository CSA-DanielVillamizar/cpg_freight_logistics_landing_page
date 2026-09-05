using MediatR;

namespace CPG.Application.Features.Loads.Deliver;

/// <summary>
/// The carrier marks their in-transit load as delivered. Runs in one EF Core transaction:
/// the load moves to <c>Delivered</c>, an audit row is written, and after commit a
/// <c>LoadDeliveredIntegrationEvent</c> is published so billing raises the shipper invoice.
/// </summary>
public sealed record MarkLoadDeliveredCommand(Guid LoadId) : IRequest<LoadSummaryResponse>;
