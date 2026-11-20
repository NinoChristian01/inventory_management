using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Web.Controllers;

[Authorize]
[Route("notifications")]
public sealed class NotificationsController(AppDbContext db, ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!currentUser.UserId.HasValue) return Challenge();

        var items = await db.Notifications.AsNoTracking()
            .Where(n => n.RecipientId == currentUser.UserId.Value)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(100)
            .Select(n => new NotificationItemVm(
                n.Id, n.Type, n.Title, n.Body, n.LinkUrl, n.IsRead, n.CreatedAtUtc, n.ReadAtUtc))
            .ToListAsync(ct);

        ViewData["Title"] = "Notifications";
        return View(items);
    }

    [HttpPost("{id:long}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(long id, CancellationToken ct)
    {
        if (!currentUser.UserId.HasValue) return Challenge();
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == currentUser.UserId.Value, ct);
        if (n is not null && !n.IsRead)
        {
            n.IsRead = true;
            n.ReadAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("read-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        if (!currentUser.UserId.HasValue) return Challenge();
        var unread = await db.Notifications.Where(x => x.RecipientId == currentUser.UserId.Value && !x.IsRead).ToListAsync(ct);
        if (unread.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var n in unread) { n.IsRead = true; n.ReadAtUtc = now; }
            await db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Index));
    }
}

public sealed record NotificationItemVm(long Id, string Type, string Title, string? Body, string? LinkUrl, bool IsRead, DateTime CreatedAtUtc, DateTime? ReadAtUtc);