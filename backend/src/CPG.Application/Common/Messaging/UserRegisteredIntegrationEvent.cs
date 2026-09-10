using CPG.Domain.Enums;

namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a new principal completes the Tri-Sign-Up flow (T-SDD Epica 1), so
/// downstream systems (welcome email, CRM sync) can react asynchronously.
/// </summary>
public sealed record UserRegisteredIntegrationEvent : IntegrationEvent
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required UserRole Role { get; init; }
}
