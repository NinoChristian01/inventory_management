namespace ServiceDesk.Domain.Enums;

/// <summary>
/// The ten ticket statuses. Each value is also the fixed primary key seeded into
/// <c>ticket_statuses</c>, so code can compare a ticket's <c>status_id</c> without a lookup.
/// Never renumber these; add new statuses at the end.
/// </summary>
public enum TicketStatusCode : short
{
    /// <summary>Logged and waiting for triage.</summary>
    New = 1,

    /// <summary>Triaged and queued for a team.</summary>
    Open = 2,

    /// <summary>An assignee accepted it and is working on it.</summary>
    InProgress = 3,

    /// <summary>Waiting for the requester to reply or act. Pauses the SLA clock by default.</summary>
    PendingRequester = 4,

    /// <summary>Waiting for a vendor or another outside party. Pauses the SLA clock by default.</summary>
    PendingThirdParty = 5,

    /// <summary>Paused on purpose. Pauses the SLA clock by default.</summary>
    OnHold = 6,

    /// <summary>Returned to work after the requester rejected the resolution.</summary>
    Reopened = 7,

    /// <summary>A solution was given; waiting for the requester to confirm.</summary>
    Resolved = 8,

    /// <summary>Final: confirmed by the requester or closed by the system.</summary>
    Closed = 9,

    /// <summary>Final: withdrawn, invalid or not to be worked.</summary>
    Canceled = 10,
}
