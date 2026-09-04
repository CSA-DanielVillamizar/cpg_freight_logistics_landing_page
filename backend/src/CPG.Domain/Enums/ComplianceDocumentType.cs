namespace CPG.Domain.Enums;

/// <summary>Mandatory legal document classes a carrier must file (SPEC.md US-03).</summary>
public enum ComplianceDocumentType
{
    CertificateOfInsurance = 1,
    GeneralLiabilityInsurance = 2,
    FdotPermit = 3,
    OperatingAuthority = 4,
    W9 = 5,
}
