using MediatR;

namespace CPG.Domain.Common;
#pragma warning disable CA1040 // INotification is an intentional marker interface extension

/// <summary>A fact that has happened inside the domain, dispatched after a successful commit.</summary>
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAtUtc { get; }
}

/// <summary>Convenience base recording the moment the event was raised.</summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
