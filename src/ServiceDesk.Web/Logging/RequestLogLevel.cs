using Serilog.Events;

namespace ServiceDesk.Web.Logging;

/// <summary>
/// Chooses the level of each "HTTP GET /path responded 200" line. Failures stand out; health probes
/// and static files (CSS, scripts, fonts) are kept out of the normal log so real traffic is readable.
/// </summary>
public static class RequestLogLevel
{
    private static readonly string[] StaticPrefixes = ["/lib", "/css", "/js", "/img", "/favicon"];

    public static LogEventLevel For(HttpContext context, double elapsedMilliseconds, Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (exception is not null || context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        var path = context.Request.Path;
        if (path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            return LogEventLevel.Verbose;
        }

        if (StaticPrefixes.Any(prefix => path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return LogEventLevel.Debug;
        }

        return LogEventLevel.Information;
    }
}
