namespace CPG.Domain.Enums;

/// <summary>Carrier compliance verification state (SPEC.md US-03).</summary>
public enum ComplianceStatus
{
    PendingCompliance = 1,
    UnderReview = 2,
    Verified = 3,
    Rejected = 4,
}
