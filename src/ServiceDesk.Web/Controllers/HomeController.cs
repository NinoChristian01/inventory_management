using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Common;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.ViewModels;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// The dashboard. Until role dashboards arrive it shows system status: health, version,
/// environment and the organization's time zone.
/// </summary>
public sealed class HomeController(
    HealthCheckService healthChecks,
    IClock clock,
    IOptions<AppOptions> options,
    IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var app = options.Value;
        var nowUtc = clock.UtcNow;
        var offset = clock.TimeZone.GetUtcOffset(nowUtc);

        var model = new SystemStatusViewModel
        {
            AppName = app.Name,
            OrganizationName = app.OrganizationName,
            Version = AppVersion.Current,
            EnvironmentName = environment.EnvironmentName,
            TimeZoneId = clock.TimeZone.Id,
            UtcOffset = $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset:hh\\:mm}",
            NowUtc = nowUtc,
            OverallStatus = report.Status.ToString(),
            Checks = [.. report.Entries.Select(e => new HealthCheckRow(e.Key, e.Value.Status.ToString(), e.Value.Description))],
            Runtime = environment.IsDevelopment() ? RuntimeInformation.FrameworkDescription : null,
        };

        return View(model);
    }
}
