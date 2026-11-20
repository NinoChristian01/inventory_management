using Microsoft.Net.Http.Headers;

namespace ServiceDesk.Web.Middleware;

/// <summary>
/// Adds security headers to every response, including error pages. The headers are written just
/// before the response starts, so they survive the exception handler clearing the response.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    private readonly string _contentSecurityPolicy = ContentSecurityPolicy.Build(environment.IsDevelopment());

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Response.OnStarting(ApplyHeaders, (context.Response, _contentSecurityPolicy));
        return next(context);
    }

    private static Task ApplyHeaders(object state)
    {
        var (response, contentSecurityPolicy) = ((HttpResponse, string))state;
        var headers = response.Headers;

        headers.ContentSecurityPolicy = contentSecurityPolicy;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Pages can show personal data, so browsers mustn't cache them. Static files set their own caching.
        if (!headers.ContainsKey(HeaderNames.CacheControl))
        {
            headers.CacheControl = "no-store, max-age=0";
        }

        return Task.CompletedTask;
    }
}
