namespace ServiceDesk.Web.ErrorHandling;

/// <summary>Friendly title and message for the error page, by HTTP status code.</summary>
public static class StatusCodeText
{
    public static (string Title, string Message) For(int statusCode) => statusCode switch
    {
        400 => ("Bad request", "The request couldn't be processed. Go back, check what you entered and try again."),
        401 => ("Sign-in required", "Sign in to continue."),
        403 => ("No access", "You don't have permission to open this page. If you need it, ask an administrator."),
        404 => ("Page not found", "We couldn't find that page. Check the address, or go to the dashboard."),
        405 => ("Not allowed", "That action isn't available on this page."),
        408 => ("Request timed out", "The request took too long. Try again."),
        413 => ("File too large", "That upload is larger than allowed. Choose a smaller file and try again."),
        429 => ("Too many requests", "You've made too many requests in a short time. Wait a minute, then try again."),
        >= 500 => ("Something went wrong", ExceptionStatusMapper.UnexpectedMessage),
        _ => ("Something went wrong", "The request couldn't be completed. Go back and try again."),
    };
}
