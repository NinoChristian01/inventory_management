using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ServiceDesk.Web.ErrorHandling;

/// <summary>
/// Turns unhandled exceptions in API calls into problem details (JSON). Page requests are left
/// to the exception handler middleware, which re-runs them against /error to show the friendly page.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!ApiRequest.IsApi(httpContext))
        {
            return false;
        }

        var mapping = ExceptionStatusMapper.Map(exception);
        if (mapping.IsExpected)
        {
            LogExpected(logger, httpContext.Request.Path, mapping.StatusCode, exception.Message);
        }
        else
        {
            LogUnexpected(logger, exception, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = mapping.StatusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = mapping.StatusCode,
                Title = mapping.Title,
                Detail = mapping.Message,
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "API request {Path} ended with {StatusCode}: {Reason}")]
    private static partial void LogExpected(ILogger logger, PathString path, int statusCode, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in API request {Path}")]
    private static partial void LogUnexpected(ILogger logger, Exception exception, PathString path);
}
