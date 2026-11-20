using System.Globalization;
using ServiceDesk.Domain.Enums;

namespace ServiceDesk.Web.Helpers;

/// <summary>How a local time is written on screen.</summary>
public enum LocalTimeFormat
{
    /// <summary>Nov 20, 2026</summary>
    Date,

    /// <summary>Nov 20, 2026 10:45 AM</summary>
    DateTime,

    /// <summary>10:45 AM</summary>
    Time,
}

/// <summary>
/// Presentation-only mappings: CSS classes, default labels and date formats. No business rules.
/// Labels here are the defaults; once statuses live in the database, an admin-edited name wins.
/// </summary>
public static class DisplayHelpers
{
    public static string StatusLabel(TicketStatusCode code) => code switch
    {
        TicketStatusCode.New => "New",
        TicketStatusCode.Open => "Open",
        TicketStatusCode.InProgress => "In Progress",
        TicketStatusCode.PendingRequester => "Pending Requester",
        TicketStatusCode.PendingThirdParty => "Pending Third Party",
        TicketStatusCode.OnHold => "On Hold",
        TicketStatusCode.Reopened => "Reopened",
        TicketStatusCode.Resolved => "Resolved",
        TicketStatusCode.Closed => "Closed",
        TicketStatusCode.Canceled => "Canceled",
        _ => "Unknown",
    };

    public static string StatusCssClass(TicketStatusCode code) => code switch
    {
        TicketStatusCode.New => "sd-status-new",
        TicketStatusCode.Open => "sd-status-open",
        TicketStatusCode.InProgress => "sd-status-in-progress",
        TicketStatusCode.PendingRequester => "sd-status-pending-requester",
        TicketStatusCode.PendingThirdParty => "sd-status-pending-third-party",
        TicketStatusCode.OnHold => "sd-status-on-hold",
        TicketStatusCode.Reopened => "sd-status-reopened",
        TicketStatusCode.Resolved => "sd-status-resolved",
        TicketStatusCode.Closed => "sd-status-closed",
        TicketStatusCode.Canceled => "sd-status-canceled",
        _ => "sd-status-unknown",
    };

    public static string PriorityLabel(PriorityCode code) => code switch
    {
        PriorityCode.Low => "Low",
        PriorityCode.Medium => "Medium",
        PriorityCode.High => "High",
        PriorityCode.Critical => "Critical",
        _ => "Unknown",
    };

    public static string PriorityCssClass(PriorityCode code) => code switch
    {
        PriorityCode.Low => "sd-priority-low",
        PriorityCode.Medium => "sd-priority-medium",
        PriorityCode.High => "sd-priority-high",
        PriorityCode.Critical => "sd-priority-critical",
        _ => "sd-priority-unknown",
    };

    public static string SlaCssClass(SlaState state) => state switch
    {
        SlaState.OnTrack => "sd-sla-on-track",
        SlaState.AtRisk => "sd-sla-at-risk",
        SlaState.Breached => "sd-sla-breached",
        SlaState.Paused => "sd-sla-paused",
        SlaState.Met => "sd-sla-met",
        _ => "sd-sla-unknown",
    };

    /// <summary>Bootstrap Icons name (without the "bi-" prefix) for each SLA state.</summary>
    public static string SlaIcon(SlaState state) => state switch
    {
        SlaState.OnTrack => "clock",
        SlaState.AtRisk => "hourglass-split",
        SlaState.Breached => "exclamation-octagon",
        SlaState.Paused => "pause-circle",
        SlaState.Met => "check-circle",
        _ => "question-circle",
    };

    public static string FormatLocal(DateTime local, LocalTimeFormat format) => format switch
    {
        LocalTimeFormat.Date => local.ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
        LocalTimeFormat.Time => local.ToString("h:mm tt", CultureInfo.InvariantCulture),
        _ => local.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture),
    };

    /// <summary>Up to two initials for an avatar: "Juan Dela Cruz" gives "JC".</summary>
    public static string Initials(string? fullName)
    {
        var parts = (fullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => char.ToUpperInvariant(parts[0][0]).ToString(),
            _ => string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[^1][0])),
        };
    }
}
