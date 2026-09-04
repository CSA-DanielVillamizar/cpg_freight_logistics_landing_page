namespace CPG.Domain.Common;

/// <summary>
/// Marks an entity whose create/update audit stamps are maintained automatically by the
/// persistence layer (see Infrastructure AuditableEntityInterceptor).
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; set; }

    string? CreatedBy { get; set; }

    DateTimeOffset? LastModifiedAtUtc { get; set; }

    string? LastModifiedBy { get; set; }
}
