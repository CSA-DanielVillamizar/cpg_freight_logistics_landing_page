using FluentValidation;

namespace CPG.Application.Features.Compliance.Upload;

public sealed class UploadComplianceDocumentCommandValidator : AbstractValidator<UploadComplianceDocumentCommand>
{
    /// <summary>Strict 5&#160;MB cap (SPEC.md US-03 task brief).</summary>
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
        ["application/pdf", "image/jpeg", "image/jpg"];

    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg"];

    public UploadComplianceDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType).IsInEnum();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(400)
            .Must(HaveAllowedExtension)
            .WithMessage("Only .pdf, .jpg or .jpeg files are accepted.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Content type must be application/pdf or image/jpeg.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("The file is empty.")
            .LessThanOrEqualTo(MaxSizeBytes).WithMessage("The file exceeds the 5 MB limit.");

        RuleFor(x => x.Content).NotNull();
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
