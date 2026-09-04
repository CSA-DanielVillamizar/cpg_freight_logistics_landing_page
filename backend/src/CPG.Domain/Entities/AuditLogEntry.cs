using CPG.Domain.Common;

namespace CPG.Domain.Entities;

/// <summary>
/// Immutable audit trail row persisted in PostgreSQL. Required by SPEC.md US-03
/// ("an audit log entry must be recorded in PostgreSQL with timestamp and user ID").
/// </summary>
public class AuditLogEntry : Entity
{
    public required string Action { get; set; }

    public required string EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? UserId { get; set; }

    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Optional serialized JSON payload with additional context.</summary>
    public string? DataJson { get; set; }

    /// <summary>W3C trace id (<c>traceparent</c>) for distributed correlation (SPEC.md section 2).</summary>
    public string? TraceId { get; set; }
}
