namespace CPG.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated principal lacks the role required for an operation.
/// Surfaces as HTTP 403 with the message "Access denied" (SPEC.md US-01).
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Access denied")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
