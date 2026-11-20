using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Application.Security;
using ServiceDesk.Application.Tickets;

namespace ServiceDesk.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tickets")]
public sealed class ApiTicketsController(ITicketService tickets) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] short? statusId, [FromQuery] short? priorityId,
        [FromQuery] int? teamId, [FromQuery] bool? openOnly, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "updated_desc", CancellationToken ct = default)
    {
        var (items, total) = await tickets.GetTicketsAsync(new TicketQuery(search, statusId, priorityId, teamId, openOnly, page, pageSize, sort), ct);
        return Ok(new
        {
            items,
            totalCount = total,
            page = Math.Max(1, page),
            pageSize = Math.Clamp(pageSize, 1, 100)
        });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var details = await tickets.GetDetailsAsync(id, ct);
        if (details is null) return NotFound();

        var t = details.Ticket;
        var dto = new
        {
            t.Id,
            t.TicketNumber,
            t.Subject,
            t.Description,
            status = new { t.Status.Id, t.Status.Code, t.Status.Name, t.Status.IsOpen, t.Status.IsTerminal },
            priority = new { t.Priority.Id, t.Priority.Code, t.Priority.Name, t.Priority.Color },
            category = t.Category is null ? null : new { t.Category.Id, t.Category.Name },
            team = t.AssignedTeam is null ? null : new { t.AssignedTeam.Id, t.AssignedTeam.Name, t.AssignedTeam.Code },
            requester = new { id = t.RequesterId, name = t.Requester?.FullName ?? t.RequesterNameSnapshot },
            primaryAssignee = t.PrimaryAssignee is null ? null : new { t.PrimaryAssignee.Id, name = t.PrimaryAssignee.FullName },
            t.CreatedAtUtc,
            t.UpdatedAtUtc,
            t.FirstResponseAtUtc,
            t.ResponseDueAtUtc,
            t.ResolutionDueAtUtc,
            t.ResolvedAtUtc,
            t.ClosedAtUtc,
            comments = details.Comments.Select(c => new
            {
                c.Id,
                c.Visibility,
                c.CommentType,
                c.Body,
                c.CreatedAtUtc,
                author = c.Author is null ? null : new { c.Author.Id, name = c.Author.FullName },
                attachments = details.Attachments.Where(a => a.CommentId == c.Id).Select(a => new
                {
                    a.Id,
                    a.OriginalFileName,
                    a.ContentType,
                    a.FileSizeBytes,
                    a.IsInternal,
                    downloadUrl = Url.Action("DownloadAttachment", "Tickets", new { id = t.Id, attachmentId = a.Id })
                })
            }),
            attachments = details.Attachments.Where(a => a.CommentId is null).Select(a => new
            {
                a.Id,
                a.OriginalFileName,
                a.ContentType,
                a.FileSizeBytes,
                a.IsInternal,
                downloadUrl = Url.Action("DownloadAttachment", "Tickets", new { id = t.Id, attachmentId = a.Id })
            }),
            activity = details.Activity.Select(a => new
            {
                a.Id,
                a.EventType,
                a.FieldName,
                a.OldValue,
                a.NewValue,
                a.CreatedAtUtc,
                actor = a.ActorName
            }),
            watchers = details.Watchers.Select(w => new { w.UserId, name = w.User?.FullName, w.AddedAtUtc })
        };
        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Tickets.Create)]
    public async Task<IActionResult> Create([FromBody] ApiCreateTicketRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest(new { error = "Invalid request." });
        var ticket = await tickets.CreateAsync(new CreateTicketRequest(request.Subject ?? string.Empty, request.Description ?? string.Empty, request.PriorityId, request.CategoryId, request.AssignedTeamId, request.RequesterId), ct);
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, new { ticket.Id, ticket.TicketNumber });
    }

    [HttpPost("{id:long}/comments")]
    public async Task<IActionResult> AddComment(long id, [FromBody] ApiAddCommentRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest(new { error = "Invalid request." });
        var commentId = await tickets.AddCommentAsync(new AddCommentRequest(id, request.Body ?? string.Empty, request.IsInternal), ct);
        return Ok(new { id = commentId });
    }

    [HttpPost("{id:long}/status")]
    [Authorize(Policy = Permissions.Tickets.Workflow)]
    public async Task<IActionResult> ChangeStatus(long id, [FromBody] ApiChangeStatusRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest(new { error = "Invalid request." });
        await tickets.ChangeStatusAsync(new ChangeStatusRequest(id, request.NewStatusId, request.Note, request.ResolutionCode, request.ResolutionSummary), ct);
        return NoContent();
    }

    [HttpPost("{id:long}/assign")]
    public async Task<IActionResult> Assign(long id, [FromBody] ApiAssignRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest(new { error = "Invalid request." });
        await tickets.AssignAsync(new AssignTicketRequest(id, request.AssigneeUserId, request.TeamId, request.MakePrimary ?? true), ct);
        return NoContent();
    }

    [HttpPost("{id:long}/watch")]
    public async Task<IActionResult> Watch(long id, [FromBody] ApiWatchRequest request, CancellationToken ct)
    {
        await tickets.SetWatchingAsync(id, request?.Watch ?? true, ct);
        return NoContent();
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct)
    {
        var statuses = await tickets.GetStatusesAsync(ct);
        var priorities = await tickets.GetPrioritiesAsync(ct);
        var categories = await tickets.GetCategoriesAsync(ct);
        var teams = await tickets.GetAssignableTeamsAsync(ct);
        return Ok(new
        {
            statuses = statuses.Select(s => new { s.Id, s.Code, s.Name, s.IsOpen, s.IsTerminal, s.PausesSla, s.SortOrder }),
            priorities = priorities.Select(p => new { p.Id, p.Code, p.Name, p.SortOrder, color = p.Color }),
            categories = categories.Select(c => new { c.Id, c.Name, c.DefaultTeamId, c.ParentId }),
            teams = teams.Select(t => new { t.Id, t.Code, t.Name, t.IsServiceTeam })
        });
    }
}

public sealed record ApiCreateTicketRequest(string? Subject, string? Description, short PriorityId, int? CategoryId, int? AssignedTeamId, long? RequesterId);
public sealed record ApiAddCommentRequest(string? Body, bool IsInternal);
public sealed record ApiChangeStatusRequest(short NewStatusId, string? Note, string? ResolutionCode, string? ResolutionSummary);
public sealed record ApiAssignRequest(long? AssigneeUserId, int? TeamId, bool? MakePrimary);
public sealed record ApiWatchRequest(bool Watch);