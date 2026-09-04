namespace CPG.Infrastructure.Identity;

/// <summary>Binds the <c>Jwt</c> configuration section (SPEC.md US-01).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "cpg-enterprises";

    public string Audience { get; set; } = "cpg-enterprises-clients";

    /// <summary>Symmetric signing key. Supplied via secret/env in every real environment.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}
