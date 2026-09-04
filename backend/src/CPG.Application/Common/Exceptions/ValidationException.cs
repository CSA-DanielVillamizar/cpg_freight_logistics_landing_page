using FluentValidation.Results;

namespace CPG.Application.Common.Exceptions;

/// <summary>Thrown by <c>ValidationBehaviour</c> when one or more FluentValidation rules fail.</summary>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
        => Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
