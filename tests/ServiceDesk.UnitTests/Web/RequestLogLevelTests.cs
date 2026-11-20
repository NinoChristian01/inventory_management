using Microsoft.AspNetCore.Http;
using Serilog.Events;
using ServiceDesk.Web.Logging;

namespace ServiceDesk.UnitTests.Web;

public sealed class RequestLogLevelTests
{
    [Theory]
    [InlineData("/", 200, LogEventLevel.Information)]
    [InlineData("/tickets/42", 404, LogEventLevel.Information)]
    [InlineData("/tickets", 500, LogEventLevel.Error)]
    [InlineData("/health", 200, LogEventLevel.Verbose)]
    [InlineData("/css/site.css", 200, LogEventLevel.Debug)]
    [InlineData("/lib/bootstrap/css/bootstrap.min.css", 200, LogEventLevel.Debug)]
    public void Picks_a_level_by_path_and_status(string path, int statusCode, LogEventLevel expected)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.StatusCode = statusCode;

        Assert.Equal(expected, RequestLogLevel.For(context, 12.5, null));
    }

    [Fact]
    public void Exceptions_are_always_errors()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        Assert.Equal(LogEventLevel.Error, RequestLogLevel.For(context, 1, new InvalidOperationException("boom")));
    }
}
