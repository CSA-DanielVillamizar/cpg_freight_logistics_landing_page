namespace CPG.Application.Common.Models;

/// <summary>Outcome of an operation that can fail without throwing.</summary>
public class Result
{
    protected Result(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public bool Failed => !Succeeded;

    public IReadOnlyList<string> Errors { get; }

    public static Result Success() => new(true, []);

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => new(value, true, []);

    public static Result<T> Failure<T>(params string[] errors) => new(default, false, errors);
}

/// <summary>Outcome carrying a value on success.</summary>
public sealed class Result<T> : Result
{
    internal Result(T? value, bool succeeded, IReadOnlyList<string> errors)
        : base(succeeded, errors)
        => Value = value;

    public T? Value { get; }
}
