using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Authorization;
using ServiceDesk.Application.Common;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Infrastructure.Persistence;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.ViewModels;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Dashboard: ticket overview for the current user plus system status.
/// </summary>
public sealed class HomeController(
    HealthCheckService healthChecks,
    IClock clock,
    IOptions<AppOptions> options,
    IWebHostEnvironment environment,
    AppDbContext db,
    ITicketAccessScope accessScope,
    ICurrentUser currentUser) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var app = options.Value;
        var nowUtc = clock.UtcNow;
        var offset = clock.TimeZone.GetUtcOffset(nowUtc);

        var system = new SystemStatusViewModel
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

        var stats = new TicketStatsViewModel();
        var recent = Array.Empty<DashboardTicketItem>();

        if (currentUser.IsAuthenticated)
        {
            var scope = await accessScope.GetScopeAsync(cancellationToken);
            var q = accessScope.Apply(
                db.Tickets.AsNoTracking().Include(t => t.Status).Include(t => t.Priority).Include(t => t.Requester),
                scope);

            var todayUtc = nowUtc.Date;
            var openQuery = q.Where(t => t.Status.IsOpen);
            var openCount = await openQuery.CountAsync(cancellationToken);
            var myOpenCount = currentUser.UserId.HasValue
                ? await openQuery.CountAsync(t => t.PrimaryAssigneeId == currentUser.UserId.Value || t.RequesterId == currentUser.UserId.Value, cancellationToken)
                : 0;
            var unassignedCount = await openQuery.CountAsync(t => t.PrimaryAssigneeId == null, cancellationToken);
            var overdueCount = await openQuery.CountAsync(t => (t.ResolutionDueAtUtc ?? t.ResponseDueAtUtc) != null && (t.ResolutionDueAtUtc ?? t.ResponseDueAtUtc) < nowUtc, cancellationToken);
            var resolvedTodayCount = await q.CountAsync(t => t.ResolvedAtUtc != null && t.ResolvedAtUtc >= todayUtc, cancellationToken);

            stats = new TicketStatsViewModel
            {
                OpenCount = openCount,
                MyOpenCount = myOpenCount,
                UnassignedCount = unassignedCount,
                OverdueCount = overdueCount,
                ResolvedTodayCount = resolvedTodayCount
            };

            recent = await q.OrderByDescending(t => t.UpdatedAtUtc)
                .Take(8)
                .Select(t => new DashboardTicketItem(
                    t.Id,
                    t.TicketNumber,
                    t.Subject,
                    t.Status.Name,
                    t.Status.Code,
                    t.Priority.Name,
                    t.Priority.Code,
                    t.Requester != null ? t.Requester.FullName : (t.RequesterNameSnapshot ?? "Unknown"),
                    t.UpdatedAtUtc,
                    t.ResolutionDueAtUtc ?? t.ResponseDueAtUtc))
                .ToArrayAsync(cancellationToken);
        }

        var model = new DashboardViewModel
        {
            SystemStatus = system,
            Stats = stats,
            RecentTickets = recent
        };

        ViewData["Title"] = "Dashboard";
        return View(model);
    }
}
