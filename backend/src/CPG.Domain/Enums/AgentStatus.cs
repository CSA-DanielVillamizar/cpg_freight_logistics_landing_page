namespace CPG.Domain.Enums;

/// <summary>Independent agent activation state (T-SDD Epica 1 / Epica 4).</summary>
public enum AgentStatus
{
    PendingActivation = 1,
    Active = 2,
    Suspended = 3,
}
