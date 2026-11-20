using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Web.ViewComponents;

public sealed class NotificationsBadgeViewComponent(AppDbContext db, ICurrentUser currentUser) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = 0;
        if (currentUser.IsAuthenticated && currentUser.UserId.HasValue)
        {
            count = await db.Notifications.AsNoTracking()
                .CountAsync(n => n.RecipientId == currentUser.UserId.Value && !n.IsRead, HttpContext.RequestAborted);
        }
        return View(count);
    }
}