namespace ServiceDesk.Web.Middleware;

/// <summary>
/// Redirects authenticated users who must change their password to the change-password page.
/// </summary>
public sealed class ForcePasswordChangeMiddleware(RequestDelegate next)
{
    private static readonly PathString ChangePasswordPath = new("/account/change-password");
    private static readonly PathString LoginPath = new("/account/login");
    private static readonly PathString LogoutPath = new("/account/logout");

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true &&
            string.Equals(user.FindFirst("must_change_password")?.Value, "true", StringComparison.OrdinalIgnoreCase))
        {
            var path = context.Request.Path;
            var allowed = path.StartsWithSegments(ChangePasswordPath, StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments(LogoutPath, StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments(LoginPath, StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments("/error", StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
                          path.StartsWithSegments("/img", StringComparison.OrdinalIgnoreCase);

            if (!allowed)
            {
                context.Response.Redirect(ChangePasswordPath);
                return;
            }
        }

        await next(context);
    }
}
