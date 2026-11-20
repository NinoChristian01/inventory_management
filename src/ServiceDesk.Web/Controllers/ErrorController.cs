using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Web.ErrorHandling;
using ServiceDesk.Web.ViewModels;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Friendly error pages. The exception handler and status code pages re-run failed requests
/// here with the original HTTP method, so actions accept any method and skip antiforgery checks
/// (a failed antiforgery check must still be able to show its error page).
/// </summary>
[AllowAnonymous]
[Route("error")]
[IgnoreAntiforgeryToken]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ErrorController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var mapping = ExceptionStatusMapper.Map(feature?.Error);
        Response.StatusCode = mapping.StatusCode;

        return View("Error", new ErrorViewModel
        {
            StatusCode = mapping.StatusCode,
            Title = mapping.Title,
            Message = mapping.Message,
            RequestId = HttpContext.TraceIdentifier,
        });
    }

    [Route("{statusCode:int}")]
    public IActionResult Status(int statusCode)
    {
        // Only error codes get an error page; anything else typed into the address is "not found".
        if (statusCode is < 400 or > 599)
        {
            statusCode = StatusCodes.Status404NotFound;
        }

        var (title, message) = StatusCodeText.For(statusCode);
        Response.StatusCode = statusCode;

        return View("Error", new ErrorViewModel
        {
            StatusCode = statusCode,
            Title = title,
            Message = message,
            RequestId = HttpContext.TraceIdentifier,
        });
    }
}
