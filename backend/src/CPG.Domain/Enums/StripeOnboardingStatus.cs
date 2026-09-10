namespace CPG.Domain.Enums;

/// <summary>Stripe Connect onboarding state, shared by Carrier and Agent (T-SDD Epica 2B).</summary>
public enum StripeOnboardingStatus
{
    NotStarted = 1,
    PendingVerification = 2,
    Active = 3,
    Restricted = 4,
}
