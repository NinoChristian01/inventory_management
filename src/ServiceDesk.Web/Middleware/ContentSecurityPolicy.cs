namespace ServiceDesk.Web.Middleware;

/// <summary>
/// Builds the Content-Security-Policy header. Scripts may only come from this site, so no view
/// may contain an inline &lt;script&gt;; pass data to scripts through data-* attributes or
/// &lt;script type="application/json"&gt; blocks instead. Inline styles stay allowed because
/// Bootstrap and DataTables set them.
/// </summary>
public static class ContentSecurityPolicy
{
    public static string Build(bool isDevelopment)
    {
        // Development also allows the hot-reload and browser-refresh endpoints on other localhost ports.
        var scripts = isDevelopment ? "'self' http://localhost:* https://localhost:*" : "'self'";
        var connections = isDevelopment
            ? "'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*"
            : "'self'";

        var directives = new List<string>
        {
            "default-src 'self'",
            "base-uri 'self'",
            "object-src 'none'",
            "frame-ancestors 'none'",
            "form-action 'self'",
            "img-src 'self' data:",
            "font-src 'self'",
            "style-src 'self' 'unsafe-inline'",
            $"script-src {scripts}",
            $"connect-src {connections}",
        };

        if (!isDevelopment)
        {
            directives.Add("upgrade-insecure-requests");
        }

        return string.Join("; ", directives);
    }
}
