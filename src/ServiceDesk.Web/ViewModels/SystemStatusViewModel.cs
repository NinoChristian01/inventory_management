namespace ServiceDesk.Web.ViewModels;

/// <summary>The dashboard's system status panel.</summary>
public sealed class SystemStatusViewModel
{
    public required string AppName { get; init; }

    public required string OrganizationName { get; init; }

    public required string Version { get; init; }

    public required string EnvironmentName { get; init; }

    public required string TimeZoneId { get; init; }

    /// <summary>For example "UTC+08:00".</summary>
    public required string UtcOffset { get; init; }

    public DateTime NowUtc { get; init; }

    public required string OverallStatus { get; init; }

    public required IReadOnlyList<HealthCheckRow> Checks { get; init; }

    /// <summary>.NET runtime description; filled in Development only.</summary>
    public string? Runtime { get; init; }
}

/// <summary>One health check result.</summary>
public sealed record HealthCheckRow(string Name, string Status, string? Description);
