namespace CPG.Application.Common.Exceptions;

/// <summary>Thrown when a requested aggregate does not exist. Surfaces as HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
