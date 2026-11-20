namespace ServiceDesk.Web.ViewModels;

public sealed class DashboardViewModel
{
    public required SystemStatusViewModel SystemStatus { get; init; }
    public required TicketStatsViewModel Stats { get; init; }
    public required IReadOnlyList<DashboardTicketItem> RecentTickets { get; init; }
}

public sealed class TicketStatsViewModel
{
    public int OpenCount { get; init; }
    public int MyOpenCount { get; init; }
    public int UnassignedCount { get; init; }
    public int OverdueCount { get; init; }
    public int ResolvedTodayCount { get; init; }
}

public sealed record DashboardTicketItem(
    long Id,
    string TicketNumber,
    string Subject,
    string Status,
    string StatusCode,
    string Priority,
    string PriorityCode,
    string Requester,
    DateTime UpdatedAtUtc,
    DateTime? DueAtUtc
);
