using ServiceDesk.Web.Middleware;

namespace ServiceDesk.UnitTests.Web;

public sealed class ContentSecurityPolicyTests
{
    [Fact]
    public void Production_allows_scripts_from_this_site_only()
    {
        var policy = ContentSecurityPolicy.Build(isDevelopment: false);

        Assert.Contains("script-src 'self';", policy, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("object-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("upgrade-insecure-requests", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("localhost", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe-eval", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_scripts_are_never_allowed()
    {
        foreach (var isDevelopment in new[] { true, false })
        {
            var scriptDirective = ContentSecurityPolicy.Build(isDevelopment)
                .Split(';', StringSplitOptions.TrimEntries)
                .Single(d => d.StartsWith("script-src", StringComparison.Ordinal));

            Assert.DoesNotContain("unsafe-inline", scriptDirective, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Development_allows_hot_reload_connections()
    {
        var policy = ContentSecurityPolicy.Build(isDevelopment: true);

        Assert.Contains("ws://localhost:*", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("upgrade-insecure-requests", policy, StringComparison.Ordinal);
    }
}
