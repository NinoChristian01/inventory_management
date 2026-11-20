using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Web.Controllers;

[Authorize(Roles = "Admin,TeamLead,Agent")]
[Route("teams")]
public sealed class TeamsController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var teams = await db.Teams.AsNoTracking()
            .OrderByDescending(t => t.IsServiceTeam)
            .ThenBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Code,
                t.Name,
                t.IsServiceTeam,
                t.IsActive,
                t.IsDeleted,
                MemberCount = db.TeamMembers.Count(tm => tm.TeamId == t.Id && tm.LeftAtUtc == null)
            })
            .ToListAsync(ct);

        ViewData["Title"] = "Teams";
        return View(teams);
    }
}
