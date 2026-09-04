namespace CPG.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication fails (bad credentials, expired/invalid refresh token).
/// Surfaces as HTTP 401 (SPEC.md US-01).
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Invalid credentials")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
