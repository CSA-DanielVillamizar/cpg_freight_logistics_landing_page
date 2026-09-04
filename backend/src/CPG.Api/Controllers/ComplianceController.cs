using CPG.Api.Infrastructure;
using CPG.Application.Common.Exceptions;
using CPG.Application.Features.Compliance;
using CPG.Application.Features.Compliance.GetStatus;
using CPG.Application.Features.Compliance.Upload;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Carrier compliance document upload and review (SPEC.md US-03).</summary>
[Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
public sealed class ComplianceController(ISender sender) : ApiControllerBase
{
    private const long MaxUploadBytes = UploadComplianceDocumentCommandValidator.MaxSizeBytes;

    /// <summary>The authenticated carrier's compliance snapshot.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ComplianceStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ComplianceStatusResponse>> GetStatus(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetComplianceStatusQuery(), cancellationToken));

    /// <summary>
    /// Upload a legal document (COI, insurance, FDOT permit). Stored in blob storage; the
    /// carrier record moves to <c>Under Review</c>, an audit row is written, and a RabbitMQ
    /// event notifies the commercial team (SPEC.md US-03).
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes + (256 * 1024))]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes + (256 * 1024))]
    [ProducesResponseType(typeof(UploadComplianceDocumentResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UploadComplianceDocumentResult>> Upload(
        IFormFile file,
        [FromForm] ComplianceDocumentType documentType,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure("file", "A non-empty file is required."),
            ]);
        }

        await using var stream = file.OpenReadStream();

        var result = await sender.Send(
            new UploadComplianceDocumentCommand
            {
                DocumentType = documentType,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                Content = stream,
            },
            cancellationToken);

        return Accepted(result);
    }
}
