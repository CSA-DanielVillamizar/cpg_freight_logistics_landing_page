namespace CPG.Domain.Common;

/// <summary>Base type for all persistent domain entities. Identity is a <see cref="Guid"/>.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
