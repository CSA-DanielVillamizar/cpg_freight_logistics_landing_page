using CPG.Application.Common.Interfaces;

namespace CPG.Infrastructure.Services;

/// <summary>System-clock implementation of <see cref="IDateTimeProvider"/>.</summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
