using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>
/// A freight load posted to the board. Creation is idempotent via <c>Idempotency-Key</c>
/// and assignment is guarded by optimistic concurrency (SPEC.md section 2).
/// </summary>
public class Load : AggregateRoot, IAuditableEntity, IHasRowVersion
{
    public required string Reference { get; set; }

    public required ServiceType ServiceType { get; set; }

    public required string OriginZip { get; set; }

    public required string DestinationZip { get; set; }

    public required int WeightLbs { get; set; }

    public LoadStatus Status { get; set; } = LoadStatus.Draft;

    public Guid? AssignedCarrierId { get; set; }

    /// <summary>Optimistic concurrency token mapped to PostgreSQL <c>xmin</c>.</summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
