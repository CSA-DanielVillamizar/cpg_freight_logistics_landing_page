namespace CPG.Domain.Enums;

/// <summary>Lifecycle of a niche landing-page enterprise lead (SPEC.md US-04).</summary>
public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Won = 4,
    Lost = 5,
}
