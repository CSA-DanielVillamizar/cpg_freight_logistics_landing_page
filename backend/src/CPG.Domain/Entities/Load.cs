using CPG.Domain.Common;
using CPG.Domain.Enums;
using CPG.Domain.Events;

namespace CPG.Domain.Entities;

/// <summary>
/// A freight load posted to the Carrier &amp; Shipper Load Workspace. Assignment is guarded by
/// optimistic concurrency via the PostgreSQL <c>xmin</c> token (SPEC.md section 2).
/// </summary>
public class Load : AggregateRoot, IAuditableEntity, IHasRowVersion, ISoftDelete
{
    public required string Reference { get; set; }

    public required ServiceType ServiceType { get; set; }

    public required string EquipmentType { get; set; }

    public required string OriginCity { get; set; }

    public required string OriginState { get; set; }

    public required string OriginZip { get; set; }

    public required string DestinationCity { get; set; }

    public required string DestinationState { get; set; }

    public required string DestinationZip { get; set; }

    public required int DistanceMiles { get; set; }

    public required int WeightLbs { get; set; }

    public required decimal RateUsd { get; set; }

    public required string ShipperName { get; set; }

    /// <summary>The corporate shipper user (JWT subject) that requested this load, if any.</summary>
    public Guid? ShipperUserId { get; set; }

    /// <summary>Absolute URI of the signed proof-of-delivery blob; set once the load is delivered.</summary>
    public string? PodBlobUri { get; set; }

    public required DateTimeOffset PickupAtUtc { get; set; }

    public required DateTimeOffset DeliveryAtUtc { get; set; }

    public int? TargetTemperatureF { get; set; }

    public string? SpecialInstructions { get; set; }

    public LoadStatus Status { get; set; } = LoadStatus.Available;

    /// <summary>Logical-delete flag (see <see cref="ISoftDelete"/>). The row is retained for audit.</summary>
    public bool IsDeleted { get; set; }

    public Guid? AssignedCarrierId { get; set; }

    public Carrier? AssignedCarrier { get; set; }

    /// <summary>Last GPS latitude reported by an ELD webhook, denormalized for fast map reads.</summary>
    public decimal? LastKnownLatitude { get; set; }

    public decimal? LastKnownLongitude { get; set; }

    public DateTimeOffset? LastTelemetryAtUtc { get; set; }

    /// <summary>The Independent Agent this load was published by, if any.</summary>
    public Guid? AgentId { get; set; }

    /// <summary>Pre-calculated commission shown on the Agent's dashboard at publish time.</summary>
    public decimal? ProjectedAgentCommissionUsd { get; set; }

    /// <summary>Optimistic concurrency token mapped to PostgreSQL <c>xmin</c>.</summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// A carrier claims an available load. Moves the load to <see cref="LoadStatus.Dispatched"/>,
    /// records the assignment and raises <see cref="LoadAcceptedDomainEvent"/>.
    /// </summary>
    /// <exception cref="DomainException">The load is not currently available for assignment.</exception>
    public void Accept(Guid carrierId)
    {
        if (Status != LoadStatus.Available)
        {
            throw new DomainException(
                $"Load {Reference} is not available for assignment (current status: {Status}).");
        }

        AssignedCarrierId = carrierId;
        Status = LoadStatus.Dispatched;

        RaiseDomainEvent(new LoadAcceptedDomainEvent(Id, Reference, carrierId));
    }

    /// <summary>
    /// The assigned carrier reports the truck has departed the origin. Moves the load from
    /// <see cref="LoadStatus.Dispatched"/> to <see cref="LoadStatus.InTransit"/>.
    /// </summary>
    /// <exception cref="DomainException">The load has not been dispatched to a carrier.</exception>
    public void MarkInTransit()
    {
        if (Status != LoadStatus.Dispatched)
        {
            throw new DomainException(
                $"Load {Reference} cannot depart from status {Status}; it must be Dispatched.");
        }

        Status = LoadStatus.InTransit;
    }

    /// <summary>
    /// The carrier completes the haul. Moves the load to <see cref="LoadStatus.Delivered"/> and
    /// raises <see cref="LoadDeliveredDomainEvent"/> so billing can raise the shipper invoice.
    /// </summary>
    /// <exception cref="DomainException">The load is not in transit or dispatched.</exception>
    public void MarkDelivered()
    {
        if (Status is not (LoadStatus.Dispatched or LoadStatus.InTransit))
        {
            throw new DomainException(
                $"Load {Reference} cannot be delivered from status {Status}.");
        }

        Status = LoadStatus.Delivered;

        RaiseDomainEvent(new LoadDeliveredDomainEvent(Id, Reference, ShipperUserId));
    }

    /// <summary>
    /// Records a verified GPS reading from an ELD webhook. Out-of-order or duplicate readings
    /// (by <paramref name="recordedAtUtc"/>) are silently ignored for idempotency.
    /// </summary>
    /// <exception cref="DomainException">The load is not currently trackable.</exception>
    public void UpdateTelemetry(decimal latitude, decimal longitude, DateTimeOffset recordedAtUtc, decimal? speedMph = null)
    {
        if (Status is not (LoadStatus.Dispatched or LoadStatus.InTransit))
        {
            throw new DomainException($"Load {Reference} cannot receive telemetry in status {Status}.");
        }

        if (LastTelemetryAtUtc is { } lastRecordedAtUtc && recordedAtUtc <= lastRecordedAtUtc)
        {
            return;
        }

        LastKnownLatitude = latitude;
        LastKnownLongitude = longitude;
        LastTelemetryAtUtc = recordedAtUtc;

        RaiseDomainEvent(new LoadGpsLocationUpdatedDomainEvent(Id, Reference, latitude, longitude, speedMph, recordedAtUtc));
    }

    /// <summary>
    /// Attributes a newly published load to the Independent Agent that created it, storing the
    /// pre-calculated projected commission shown on the Agent's dashboard.
    /// </summary>
    /// <exception cref="DomainException">The load is not newly published (available).</exception>
    public void AttributeToAgent(Guid agentId, decimal projectedCommissionUsd)
    {
        if (Status != LoadStatus.Available)
        {
            throw new DomainException("Solo se puede atribuir una carga recien publicada.");
        }

        AgentId = agentId;
        ProjectedAgentCommissionUsd = projectedCommissionUsd;

        RaiseDomainEvent(new LoadPublishedByAgentDomainEvent(Id, agentId, projectedCommissionUsd));
    }
}
