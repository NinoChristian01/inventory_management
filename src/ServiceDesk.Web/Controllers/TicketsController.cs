using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Application.Authorization;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Application.Security;
using ServiceDesk.Application.Tickets;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.ViewModels.Tickets;

namespace ServiceDesk.Web.Controllers;

[Authorize]
[Route("tickets")]
public sealed class TicketsController(
    ITicketService tickets,
    ITicketAccessScope accessScope,
    ICurrentUser currentUser,
    IAttachmentStorage attachmentStorage) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] TicketIndexViewModel vm, CancellationToken ct)
    {
        var query = new TicketQuery(vm.Search, vm.StatusId, vm.PriorityId, vm.TeamId, vm.OpenOnly, vm.Page <= 0 ? 1 : vm.Page, vm.PageSize, vm.Sort ?? "updated_desc");
        var (items, total) = await tickets.GetTicketsAsync(query, ct);
        vm.Items = items;
        vm.TotalCount = total;
        await PopulateIndexFiltersAsync(vm, ct);
        ViewData["Title"] = "Tickets";
        return View(vm);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var details = await tickets.GetDetailsAsync(id, ct);
        if (details is null) return NotFound();
        var scope = await accessScope.GetScopeAsync(ct);
        var vm = await BuildDetailsVmAsync(details, scope, ct);
        ViewData["Title"] = details.Ticket.TicketNumber;
        ViewData["Parent"] = "Tickets";
        ViewData["ParentUrl"] = Url.Action(nameof(Index));
        return View(vm);
    }

    [HttpGet("create")]
    [Authorize(Policy = Permissions.Tickets.Create)]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new CreateTicketViewModel();
        await PopulateCreateLookupsAsync(vm, ct);
        ViewData["Title"] = "New ticket";
        ViewData["Parent"] = "Tickets";
        ViewData["ParentUrl"] = Url.Action(nameof(Index));
        return View(vm);
    }

    [HttpPost("create")]
    [Authorize(Policy = Permissions.Tickets.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateLookupsAsync(vm, ct);
            ViewData["Title"] = "New ticket";
            return View(vm);
        }

        try
        {
            var ticket = await tickets.CreateAsync(new CreateTicketRequest(vm.Subject, vm.Description, vm.PriorityId, vm.CategoryId, vm.AssignedTeamId), ct);
            TempData.ToastSuccess($"Ticket {ticket.TicketNumber} created.");
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateLookupsAsync(vm, ct);
            ViewData["Title"] = "New ticket";
            return View(vm);
        }
    }

    [HttpPost("{id:long}/comment")]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> AddComment(long id, TicketDetailsViewModel vm, [FromForm] List<IFormFile>? attachments, CancellationToken ct)
    {
        try
        {
            var uploads = (attachments ?? [])
                .Where(f => f is { Length: > 0 })
                .Take(5)
                .Select(f => new AttachmentUpload(f.FileName, f.ContentType ?? "application/octet-stream", f.Length, f.OpenReadStream))
                .ToList();
            await tickets.AddCommentAsync(new AddCommentRequest(id, vm.Comment.Body, vm.Comment.IsInternal, uploads), ct);
            TempData.ToastSuccess(vm.Comment.IsInternal ? "Internal note added." : "Reply posted.");
        }
        catch (Exception ex)
        {
            TempData.ToastError(ex.Message);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{id:long}/attachments/{attachmentId:long}")]
    public async Task<IActionResult> DownloadAttachment(long id, long attachmentId, CancellationToken ct)
    {
        var attachment = await tickets.GetAttachmentForDownloadAsync(id, attachmentId, ct);
        if (attachment is null) return NotFound();

        var stream = await attachmentStorage.OpenReadAsync(attachment.StoredFileName, ct);
        if (stream is null) return NotFound();

        return File(stream, string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpPost("{id:long}/status")]
    [Authorize(Policy = Permissions.Tickets.Workflow)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(long id, TicketDetailsViewModel vm, CancellationToken ct)
    {
        try
        {
            await tickets.ChangeStatusAsync(new ChangeStatusRequest(id, vm.ChangeStatus.NewStatusId, vm.ChangeStatus.Note, null, vm.ChangeStatus.ResolutionSummary), ct);
            TempData.ToastSuccess("Ticket status updated.");
        }
        catch (Exception ex)
        {
            TempData.ToastError(ex.Message);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:long}/assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(long id, TicketDetailsViewModel vm, CancellationToken ct)
    {
        try
        {
            await tickets.AssignAsync(new AssignTicketRequest(id, vm.Assign.AssigneeUserId, vm.Assign.TeamId, vm.Assign.MakePrimary), ct);
            TempData.ToastSuccess("Assignment updated.");
        }
        catch (Exception ex)
        {
            TempData.ToastError(ex.Message);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:long}/watch")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleWatch(long id, bool watch, CancellationToken ct)
    {
        try
        {
            await tickets.SetWatchingAsync(id, watch, ct);
            TempData.ToastSuccess(watch ? "You are now watching this ticket." : "You are no longer watching this ticket.");
        }
        catch (Exception ex)
        {
            TempData.ToastError(ex.Message);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateIndexFiltersAsync(TicketIndexViewModel vm, CancellationToken ct)
    {
        var statuses = await tickets.GetStatusesAsync(ct);
        var priorities = await tickets.GetPrioritiesAsync(ct);
        var teams = await tickets.GetAssignableTeamsAsync(ct);
        vm.Statuses = [new SelectListItem("All statuses", ""), .. statuses.Select(s => new SelectListItem(s.Name, s.Id.ToString(), vm.StatusId == s.Id))];
        vm.Priorities = [new SelectListItem("All priorities", ""), .. priorities.Select(p => new SelectListItem(p.Name, p.Id.ToString(), vm.PriorityId == p.Id))];
        vm.Teams = [new SelectListItem("All teams", ""), .. teams.Select(t => new SelectListItem(t.Name, t.Id.ToString(), vm.TeamId == t.Id))];
    }

    private async Task PopulateCreateLookupsAsync(CreateTicketViewModel vm, CancellationToken ct)
    {
        var priorities = await tickets.GetPrioritiesAsync(ct);
        var categories = await tickets.GetCategoriesAsync(ct);
        var teams = await tickets.GetAssignableTeamsAsync(ct);
        vm.Priorities = priorities.Select(p => new SelectListItem(p.Name, p.Id.ToString(), vm.PriorityId == p.Id)).ToList();
        vm.Categories = [new SelectListItem("Select category", ""), .. categories.Select(c => new SelectListItem(c.Name, c.Id.ToString(), vm.CategoryId == c.Id))];
        vm.Teams = [new SelectListItem("Unassigned team", ""), .. teams.Select(t => new SelectListItem(t.Name, t.Id.ToString(), vm.AssignedTeamId == t.Id))];
        if (vm.PriorityId == 0)
        {
            var medium = priorities.FirstOrDefault(p => p.Code == "Medium") ?? priorities.FirstOrDefault();
            if (medium is not null) vm.PriorityId = medium.Id;
        }
    }

    private async Task<TicketDetailsViewModel> BuildDetailsVmAsync(TicketDetailsDto details, TicketAccessScopeResult scope, CancellationToken ct)
    {
        var statuses = await tickets.GetStatusesAsync(ct);
        var teams = await tickets.GetAssignableTeamsAsync(ct);
        var users = await tickets.GetAssignableUsersAsync(details.Ticket.AssignedTeamId, ct);
        var canAssign = currentUser.HasPermission(Permissions.Tickets.Assign) || currentUser.HasPermission(Permissions.Tickets.AssignSelf);
        var vm = new TicketDetailsViewModel
        {
            Details = details,
            CanViewInternalNotes = accessScope.CanViewInternalNotes(scope),
            CanAssign = canAssign,
            CanChangeStatus = currentUser.HasPermission(Permissions.Tickets.Workflow),
            IsWatching = currentUser.UserId.HasValue && details.Watchers.Any(w => w.UserId == currentUser.UserId.Value),
            Statuses = statuses.Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == details.Ticket.StatusId)).ToList(),
            Teams = [new SelectListItem("Unassigned", ""), .. teams.Select(t => new SelectListItem(t.Name, t.Id.ToString(), t.Id == details.Ticket.AssignedTeamId))],
            Users = [new SelectListItem("Unassigned", ""), .. users.Select(u => new SelectListItem(u.FullName, u.Id.ToString(), u.Id == details.Ticket.PrimaryAssigneeId))]
        };
        vm.ChangeStatus.NewStatusId = details.Ticket.StatusId;
        vm.Assign.TeamId = details.Ticket.AssignedTeamId;
        vm.Assign.AssigneeUserId = details.Ticket.PrimaryAssigneeId;
        return vm;
    }
}
