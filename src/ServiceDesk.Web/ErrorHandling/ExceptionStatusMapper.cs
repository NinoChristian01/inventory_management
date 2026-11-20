using ServiceDesk.Application.Exceptions;

namespace ServiceDesk.Web.ErrorHandling;

/// <summary>How an exception is reported: HTTP status, a short title and a message safe to show.</summary>
/// <param name="StatusCode">HTTP status code for the response.</param>
/// <param name="Title">Short heading for the error page or problem details.</param>
/// <param name="Message">Full sentence safe to show the user.</param>
/// <param name="IsExpected">True for errors users can cause and fix; these are logged as warnings.</param>
public sealed record ExceptionMapping(int StatusCode, string Title, string Message, bool IsExpected);

/// <summary>
/// Maps exceptions to what the user sees. Only the app's own exceptions carry messages written
/// for users; anything else gets a generic message so internal details never leak.
/// </summary>
public static class ExceptionStatusMapper
{
    public const string UnexpectedMessage =
        "Something went wrong on our side. Try again in a moment. If it keeps happening, contact the system administrator and give them the reference code below.";

    public static ExceptionMapping Map(Exception? exception) => exception switch
    {
        NotFoundException e => new(StatusCodes.Status404NotFound, "Not found", e.Message, true),
        ForbiddenAccessException e => new(StatusCodes.Status403Forbidden, "No access", e.Message, true),
        ConcurrencyConflictException e => new(StatusCodes.Status409Conflict, "Changed by someone else", e.Message, true),
        BusinessRuleException e => new(StatusCodes.Status409Conflict, "That can't be done", e.Message, true),
        BadHttpRequestException e => new(e.StatusCode, "Bad request", "The request couldn't be read. Check what you entered and try again.", true),
        _ => new(StatusCodes.Status500InternalServerError, "Something went wrong", UnexpectedMessage, false),
    };
}
