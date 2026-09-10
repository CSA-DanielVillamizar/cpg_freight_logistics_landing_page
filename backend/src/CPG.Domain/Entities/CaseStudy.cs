using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>An editorial case study/testimonial published on the marketing site (T-SDD Epica 3).</summary>
public class CaseStudy : AggregateRoot, IAuditableEntity
{
    public required string Title { get; set; }

    /// <summary>URL-safe unique identifier.</summary>
    public required string Slug { get; set; }

    public required ServiceType ServiceType { get; set; }

    public required string SummaryMarkdown { get; set; }

    public required string BodyMarkdown { get; set; }

    public string? HeroImageBlobUri { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
