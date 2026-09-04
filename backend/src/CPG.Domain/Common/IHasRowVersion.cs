namespace CPG.Domain.Common;

/// <summary>
/// Marks a transactional entity that participates in optimistic concurrency control.
/// Mapped to the PostgreSQL system column <c>xmin</c> (SPEC.md section 2) so no explicit
/// version column is required in the schema.
/// </summary>
public interface IHasRowVersion
{
    /// <summary>PostgreSQL <c>xmin</c> tuple version, surfaced by Npgsql as a <see cref="uint"/>.</summary>
    uint RowVersion { get; set; }
}
