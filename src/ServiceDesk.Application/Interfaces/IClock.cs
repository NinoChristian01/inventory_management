namespace ServiceDesk.Application.Interfaces;

/// <summary>
/// The one place the app reads the time. Everything is stored in UTC; the organization's time
/// zone (Asia/Manila by default) is used only for display, business hours and "today".
/// Tests replace the underlying <see cref="TimeProvider"/> to control time.
/// </summary>
public interface IClock
{
    /// <summary>Current time in UTC.</summary>
    DateTime UtcNow { get; }

    /// <summary>The organization's time zone.</summary>
    TimeZoneInfo TimeZone { get; }

    /// <summary>Current time in the organization's time zone.</summary>
    DateTime LocalNow { get; }

    /// <summary>Today's date in the organization's time zone.</summary>
    DateOnly LocalToday { get; }

    /// <summary>
    /// Converts a stored time to local time. Values with <see cref="DateTimeKind.Unspecified"/>
    /// are treated as UTC, because that is how the database stores every time.
    /// </summary>
    DateTime ToLocal(DateTime utc);

    /// <summary>Converts a local wall-clock time, such as a date picked in a filter, to UTC.</summary>
    DateTime ToUtc(DateTime local);
}
