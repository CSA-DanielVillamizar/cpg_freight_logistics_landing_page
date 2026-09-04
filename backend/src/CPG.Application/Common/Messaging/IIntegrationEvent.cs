namespace CPG.Application.Common.Messaging;

/// <summary>Contract for cross-service messages published to the broker.</summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}

/// <summary>Base record stamping identity and timestamp on every integration event.</summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
