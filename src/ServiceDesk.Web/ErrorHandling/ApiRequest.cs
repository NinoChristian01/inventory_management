namespace ServiceDesk.Web.ErrorHandling;

/// <summary>
/// Tells API calls from page requests. API calls get JSON problem details and plain 401/403
/// status codes instead of HTML error pages or sign-in redirects.
/// </summary>
public static class ApiRequest
{
    public static bool IsApi(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
