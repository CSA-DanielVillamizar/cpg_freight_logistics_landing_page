using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Events;

/// <summary>Raised when a new principal self-registers via the Tri-Sign-Up flow (T-SDD Epica 1).</summary>
public sealed record UserRegisteredDomainEvent(Guid UserId, string Email, UserRole Role) : DomainEvent;
