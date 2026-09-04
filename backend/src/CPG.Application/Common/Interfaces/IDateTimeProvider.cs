namespace CPG.Application.Common.Interfaces;

/// <summary>Abstracts the system clock so handlers and validators stay deterministic in tests.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
