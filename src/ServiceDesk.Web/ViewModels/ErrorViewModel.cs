namespace ServiceDesk.Web.ViewModels;

/// <summary>What the friendly error page shows.</summary>
public sealed class ErrorViewModel
{
    public int StatusCode { get; init; }

    public required string Title { get; init; }

    public required string Message { get; init; }

    /// <summary>Request id shown for server errors, so the log entry can be found.</summary>
    public string? RequestId { get; init; }
}
