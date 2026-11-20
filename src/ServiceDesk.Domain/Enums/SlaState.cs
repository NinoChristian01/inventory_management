namespace ServiceDesk.Domain.Enums;

/// <summary>
/// How an SLA clock stands right now, as shown on queues and ticket pages.
/// It is worked out from the clock's stage, due time and warning threshold.
/// </summary>
public enum SlaState
{
    /// <summary>Running with time to spare.</summary>
    OnTrack = 1,

    /// <summary>Running and past the warning threshold (75 % of the target by default).</summary>
    AtRisk = 2,

    /// <summary>Past its due time.</summary>
    Breached = 3,

    /// <summary>Stopped while the ticket waits on someone outside the team.</summary>
    Paused = 4,

    /// <summary>Stopped in time: the target was met.</summary>
    Met = 5,
}
