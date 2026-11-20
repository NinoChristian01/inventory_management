using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Security;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure.Persistence;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.ViewModels.Users;

namespace ServiceDesk.Web.Controllers;

[Authorize(Roles = $"{AppRoleNames.Admin},{AppRoleNames.TeamLead}")]
[Route("users")]
public sealed class UsersController(
    AppDbContext db,
    UserManager<User> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking().Include(u => u.Department).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(u =>
                u.FullName.Contains(s) ||
                (u.UserName != null && u.UserName.Contains(s)) ||
                (u.EmployeeNumber != null && u.EmployeeNumber.Contains(s)));
        }

        var users = await query.OrderBy(u => u.FullName).Take(300).ToListAsync(ct);
        var userIds = users.Select(u => u.Id).ToList();
        var roleMap = await (from ur in db.UserRoles
                             join r in db.Roles on ur.RoleId equals r.Id
                             where userIds.Contains(ur.UserId)
                             select new { ur.UserId, r.Name })
            .ToListAsync(ct);

        var model = users.Select(u => new UserListItemViewModel
        {
            Id = u.Id,
            UserName = u.UserName ?? "",
            EmployeeNumber = u.EmployeeNumber,
            FullName = u.FullName,
            Department = u.Department?.Name,
            IsActive = u.IsActive,
            MustChangePassword = u.MustChangePassword,
            Roles = string.Join(", ", roleMap.Where(x => x.UserId == u.Id).Select(x => x.Name).Distinct())
        }).ToList();

        ViewData["Title"] = "Users";
        ViewBag.Query = q;
        return View(model);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var identityUser = await userManager.FindByIdAsync(id.ToString());
        if (identityUser is null) return NotFound();

        var user = await db.Users.AsNoTracking().Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();

        var allRoles = await db.Roles.AsNoTracking().OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync(ct);
        var userRoles = await userManager.GetRolesAsync(identityUser);

        var vm = new UserDetailsViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            EmployeeNumber = user.EmployeeNumber,
            FullName = user.FullName,
            Email = user.Email,
            Department = user.Department?.Name,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            AllRoles = allRoles,
            SelectedRoles = userRoles.ToList()
        };

        ViewData["Title"] = user.FullName;
        ViewData["Parent"] = "Users";
        ViewData["ParentUrl"] = Url.Action(nameof(Index));
        return View(vm);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:long}/roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoles(long id, [FromForm] List<string> roles)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        var allowed = AppRoleNames.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var desired = (roles ?? []).Where(r => allowed.Contains(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (desired.Count == 0) desired.Add(AppRoleNames.Requester);

        var current = await userManager.GetRolesAsync(user);
        var toRemove = current.Except(desired, StringComparer.OrdinalIgnoreCase).ToArray();
        var toAdd = desired.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();

        if (toRemove.Length > 0) await userManager.RemoveFromRolesAsync(user, toRemove);
        if (toAdd.Length > 0) await userManager.AddToRolesAsync(user, toAdd);
        await userManager.UpdateSecurityStampAsync(user); // refresh claims/cookie on next validation

        TempData.ToastSuccess($"Roles updated for {user.FullName}.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:long}/activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(long id, string tempPassword)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        if (string.IsNullOrWhiteSpace(tempPassword) || tempPassword.Length < 10)
        {
            TempData.ToastError("Temporary password must be at least 10 characters and meet password policy.");
            return RedirectToAction(nameof(Index));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, token, tempPassword);
        if (!reset.Succeeded)
        {
            TempData.ToastError(string.Join(" ", reset.Errors.Select(e => e.Description)));
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = true;
        user.MustChangePassword = true;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        TempData.ToastSuccess($"User {user.FullName} activated. Temporary password: {tempPassword}");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:long}/deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(long id)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);
        TempData.ToastSuccess($"User {user.FullName} deactivated.");
        return RedirectToAction(nameof(Index));
    }
}
