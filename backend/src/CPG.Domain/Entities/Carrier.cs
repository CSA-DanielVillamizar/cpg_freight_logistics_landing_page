using CPG.Domain.Common;
using CPG.Domain.Enums;
using CPG.Domain.Events;

namespace CPG.Domain.Entities;

/// <summary>A freight carrier / owner-operator account (SPEC.md US-03).</summary>
public class Carrier : AggregateRoot, IAuditableEntity, IHasRowVersion
{
    private readonly List<ComplianceDocument> _complianceDocuments = [];

    public required string CompanyName { get; set; }

    public required Guid UserId { get; set; }

    public string? DotNumber { get; set; }

    public string? McNumber { get; set; }

    public ComplianceStatus ComplianceStatus { get; private set; } = ComplianceStatus.PendingCompliance;

    public IReadOnlyCollection<ComplianceDocument> ComplianceDocuments => _complianceDocuments.AsReadOnly();

    /// <summary>Optimistic concurrency token mapped to PostgreSQL <c>xmin</c>.</summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Files a mandatory legal document. Moves the account to <see cref="ComplianceStatus.UnderReview"/>
    /// and raises <see cref="ComplianceDocumentUploadedDomainEvent"/> (SPEC.md US-03).
    /// </summary>
    public ComplianceDocument SubmitComplianceDocument(
        ComplianceDocumentType documentType,
        string blobUri,
        string originalFileName,
        string contentType,
        long sizeBytes,
        DateTimeOffset uploadedAtUtc)
    {
        if (ComplianceStatus == ComplianceStatus.Verified)
        {
            throw new DomainException("Carrier is already verified; no further documents are required.");
        }

        var document = new ComplianceDocument
        {
            CarrierId = Id,
            DocumentType = documentType,
            BlobUri = blobUri,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Status = ComplianceStatus.UnderReview,
            CreatedAtUtc = uploadedAtUtc,
        };

        _complianceDocuments.Add(document);
        ComplianceStatus = ComplianceStatus.UnderReview;

        RaiseDomainEvent(new ComplianceDocumentUploadedDomainEvent(Id, document.Id, documentType));

        return document;
    }
}
