using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ServiceDesk.Web.Health;

/// <summary>
/// Writes /health as JSON: overall status plus each check's name, status and duration.
/// Descriptions and exception messages are left out because the endpoint is public.
/// </summary>
public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(report);

        var payload = new HealthPayload(
            report.Status.ToString(),
            Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            [.. report.Entries.Select(entry => new HealthCheckPayload(
                entry.Key,
                entry.Value.Status.ToString(),
                Math.Round(entry.Value.Duration.TotalMilliseconds, 1)))]);

        return context.Response.WriteAsJsonAsync(payload, JsonOptions, "application/json; charset=utf-8", context.RequestAborted);
    }

    private sealed record HealthPayload(string Status, double TotalDurationMs, IReadOnlyList<HealthCheckPayload> Checks);

    private sealed record HealthCheckPayload(string Name, string Status, double DurationMs);
}
