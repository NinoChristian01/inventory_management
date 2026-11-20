using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ServiceDesk.IntegrationTests;

/// <summary>
/// Hosts the real app in memory as Production, so tests see what users would see on the live
/// server: friendly error pages, production headers and no developer pages. Log files are off.
/// </summary>
public class ServiceDeskWebFactory : WebApplicationFactory<Program>
{
    protected virtual string EnvironmentName => "Production";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseEnvironment(EnvironmentName);
        builder.UseSetting("LogFiles:Enabled", "false");
    }
}

/// <summary>The same app hosted as Development, for the developer-only pages.</summary>
public sealed class DevelopmentWebFactory : ServiceDeskWebFactory
{
    protected override string EnvironmentName => "Development";
}
