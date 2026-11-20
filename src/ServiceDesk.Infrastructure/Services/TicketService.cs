using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Authorization;
using ServiceDesk.Application.Exceptions;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Application.Security;
using ServiceDesk.Application.Tickets;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Infrastructure.Services;

public sealed class TicketService(
    AppDbContext db,
    ICurrentUser currentUser,
    IClock clock,
    ITicketAccessScope accessScope,
    ITicketNumberService ticketNumbers,
    IAttachmentStorage attachmentStorage) : ITicketService
{
    public async Task<(IReadOnlyList<TicketListItemDto> Items, int TotalCount)> GetTicketsAsync(TicketQuery query, CancellationToken ct = default)
    {
        var scope = await accessScope.GetScopeAsync(ct);
        if (!scope.IsAuthenticated) throw new UnauthorizedAccessException();

        IQueryable<Ticket> q = db.Tickets
            .AsNoTracking()
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.Category)
            .Include(t => t.Requester)
            .Include(t => t.PrimaryAssignee)
            .Include(t => t.AssignedTeam)
            .Include(t => t.Assignees).ThenInclude(a => a.User);

        q = accessScope.Apply(q, scope);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t =>
                t.TicketNumber.Contains(s) ||
                t.Subject.Contains(s) ||
                (t.RequesterNameSnapshot != null && t.RequesterNameSnapshot.Contains(s)) ||
                (t.Requester != null && t.Requester.FullName.Contains(s)));
        }
        if (query.StatusId.HasValue) q = q.Where(t => t.StatusId == query.StatusId.Value);
        if (query.PriorityId.HasValue) q = q.Where(t => t.PriorityId == query.PriorityId.Value);
        if (query.TeamId.HasValue) q = q.Where(t => t.AssignedTeamId == query.TeamId.Value);
        if (query.OpenOnly == true) q = q.Where(t => t.Status.IsOpen);

        q = query.Sort switch
        {
            "created_desc" => q.OrderByDescending(t => t.CreatedAtUtc),
            "created_asc" => q.OrderBy(t => t.CreatedAtUtc),
            "updated_asc" => q.OrderBy(t => t.UpdatedAtUtc),
            "priority_desc" => q.OrderByDescending(t => t.PriorityId).ThenByDescending(t => t.UpdatedAtUtc),
            _ => q.OrderByDescending(t => t.UpdatedAtUtc)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.TicketNumber,
                t.Subject,
                t.Status.Name,
                t.Status.Code,
                t.Priority.Name,
                t.Priority.Code,
                t.Category != null ? t.Category.Name : null,
                t.Requester != null ? t.Requester.FullName : (t.RequesterNameSnapshot ?? "Unknown"),
                t.PrimaryAssignee != null ? t.PrimaryAssignee.FullName : t.Assignees.Where(a => a.UnassignedAtUtc == null).Select(a => a.User.FullName).FirstOrDefault(),
                t.AssignedTeam != null ? t.AssignedTeam.Name : null,
                t.CreatedAtUtc,
                t.UpdatedAtUtc,
                t.ResolutionDueAtUtc ?? t.ResponseDueAtUtc,
                t.Status.IsOpen
            ))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<TicketDetailsDto?> GetDetailsAsync(long id, CancellationToken ct = default)
    {
        var scope = await accessScope.GetScopeAsync(ct);
        if (!scope.IsAuthenticated) throw new UnauthorizedAccessException();

        var ticket = await db.Tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.Category)
            .Include(t => t.Requester)
            .Include(t => t.RequesterDepartment)
            .Include(t => t.AssignedTeam)
            .Include(t => t.PrimaryAssignee)
            .Include(t => t.Assignees).ThenInclude(a => a.User)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
            .Include(t => t.Watchers).ThenInclude(w => w.User)
            .Include(t => t.ActivityLogs).ThenInclude(a => a.Actor)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (ticket is null) return null;
        if (!accessScope.CanView(ticket, scope)) throw new ForbiddenAccessException("You do not have access to this ticket.");

        var canViewInternal = accessScope.CanViewInternalNotes(scope);
        var comments = ticket.Comments
            .Where(c => canViewInternal || !string.Equals(c.Visibility, "internal", StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.CreatedAtUtc)
            .ToList();
        var attachments = ticket.Attachments
            .Where(a => canViewInternal || !a.IsInternal)
            .OrderBy(a => a.CreatedAtUtc)
            .ToList();
        var activity = ticket.ActivityLogs.OrderByDescending(a => a.CreatedAtUtc).Take(200).ToList();
        var assignees = ticket.Assignees.OrderByDescending(a => a.AssignedAtUtc).ToList();
        var watchers = ticket.Watchers.OrderBy(w => w.AddedAtUtc).ToList();

        return new TicketDetailsDto(ticket, comments, activity, assignees, attachments, watchers);
    }

    public async Task<TicketDetailsDto?> GetDetailsByNumberAsync(string ticketNumber, CancellationToken ct = default)
    {
        var id = await db.Tickets.AsNoTracking().Where(t => t.TicketNumber == ticketNumber).Select(t => (long?)t.Id).FirstOrDefaultAsync(ct);
        return id.HasValue ? await GetDetailsAsync(id.Value, ct) : null;
    }

    public async Task<Ticket> CreateAsync(CreateTicketRequest request, CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) throw new UnauthorizedAccessException();
        if (!currentUser.HasPermission(Permissions.Tickets.Create)) throw new ForbiddenAccessException("You do not have permission to create tickets.");

        var subject = (request.Subject ?? string.Empty).Trim();
        var description = (request.Description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(subject)) throw new BusinessRuleException("Subject is required.");
        if (subject.Length > 255) subject = subject[..255];
        if (string.IsNullOrWhiteSpace(description)) throw new BusinessRuleException("Description is required.");

        var now = clock.UtcNow;
        var requesterId = request.RequesterId ?? currentUser.UserId.Value;
        // Only admins/agents/leads should create on behalf of others; basic guard
        if (requesterId != currentUser.UserId.Value && !(currentUser.IsInRole(AppRoleNames.Admin) || currentUser.IsInRole(AppRoleNames.Agent) || currentUser.IsInRole(AppRoleNames.TeamLead)))
        {
            requesterId = currentUser.UserId.Value;
        }

        var requester = await db.Users.FirstOrDefaultAsync(u => u.Id == requesterId, ct)
            ?? throw new NotFoundException(nameof(User), requesterId);

        var priority = await db.TicketPriorities.FirstOrDefaultAsync(p => p.Id == request.PriorityId, ct)
            ?? throw new BusinessRuleException("Invalid priority.");
        if (string.Equals(priority.Code, "Critical", StringComparison.OrdinalIgnoreCase) &&
            !(currentUser.IsInRole(AppRoleNames.Admin) || currentUser.IsInRole(AppRoleNames.Agent) || currentUser.IsInRole(AppRoleNames.TeamLead)))
        {
            throw new ForbiddenAccessException("Only agents and above can set Critical priority.");
        }

        var newStatus = await db.TicketStatuses.FirstOrDefaultAsync(s => s.Code == "New", ct)
            ?? await db.TicketStatuses.OrderBy(s => s.SortOrder).FirstAsync(ct);

        var category = request.CategoryId.HasValue
            ? await db.TicketCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, ct)
            : await db.TicketCategories.OrderBy(c => c.Id).FirstOrDefaultAsync(ct);

        var teamId = request.AssignedTeamId ?? category?.DefaultTeamId;
        var team = teamId.HasValue ? await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId.Value, ct) : null;

        var ticketNumber = await ticketNumbers.NextAsync(team?.Id, now, ct);

        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            RequesterId = requester.Id,
            RequesterDepartmentId = requester.DepartmentId,
            RequesterNameSnapshot = requester.FullName,
            Subject = subject,
            Description = description,
            StatusId = newStatus.Id,
            PriorityId = priority.Id,
            CategoryId = category?.Id,
            AssignedTeamId = team?.Id,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Version = 0
        };

        // Basic SLA assignment (calendar minutes, policy match by category or priority fallback)
        var slaPolicy = await db.SlaPolicies.AsNoTracking()
            .Where(p => p.IsActive && p.PriorityId == priority.Id && (p.CategoryId == ticket.CategoryId || p.CategoryId == null))
            .OrderByDescending(p => p.CategoryId.HasValue)
            .FirstOrDefaultAsync(ct);
        if (slaPolicy is not null)
        {
            ticket.ResponseDueAtUtc = now.AddMinutes(Math.Max(1, slaPolicy.ResponseTargetMinutes));
            ticket.ResolutionDueAtUtc = now.AddMinutes(Math.Max(1, slaPolicy.ResolutionTargetMinutes));
        }

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync(ct); // get Id

        if (slaPolicy is not null)
        {
            if (ticket.ResponseDueAtUtc.HasValue)
            {
                db.TicketSlas.Add(new TicketSla
                {
                    TicketId = ticket.Id,
                    SlaPolicyId = slaPolicy.Id,
                    Metric = "response",
                    StartedAtUtc = now,
                    DueAtUtc = ticket.ResponseDueAtUtc.Value,
                    Stage = "running",
                    IsBreached = false,
                    WarningSent = false,
                    BreachSent = false,
                    TotalPausedMinutes = 0
                });
            }
            if (ticket.ResolutionDueAtUtc.HasValue)
            {
                db.TicketSlas.Add(new TicketSla
                {
                    TicketId = ticket.Id,
                    SlaPolicyId = slaPolicy.Id,
                    Metric = "resolution",
                    StartedAtUtc = now,
                    DueAtUtc = ticket.ResolutionDueAtUtc.Value,
                    Stage = "running",
                    IsBreached = false,
                    WarningSent = false,
                    BreachSent = false,
                    TotalPausedMinutes = 0
                });
            }
        }

        db.TicketActivityLogs.Add(new TicketActivityLog
        {
            TicketId = ticket.Id,
            ActorId = currentUser.UserId,
            ActorName = currentUser.UserName,
            EventType = "created",
            FieldName = "status",
            NewValue = newStatus.Name,
            CreatedAtUtc = now
        });

        await db.SaveChangesAsync(ct);
        return ticket;
    }

    public async Task<long> AddCommentAsync(AddCommentRequest request, CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) throw new UnauthorizedAccessException();
        var scope = await accessScope.GetScopeAsync(ct);
        var ticket = await db.Tickets.Include(t => t.Status).FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);
        if (!accessScope.CanView(ticket, scope)) throw new ForbiddenAccessException("You do not have access to this ticket.");

        var isInternal = request.IsInternal;
        if (isInternal && !accessScope.CanViewInternalNotes(scope))
            throw new ForbiddenAccessException("You do not have permission to add internal notes.");

        var body = (request.Body ?? string.Empty).Trim();
        var uploads = (request.Attachments ?? []).Where(a => a.Length > 0).ToList();
        if (string.IsNullOrWhiteSpace(body) && uploads.Count == 0)
            throw new BusinessRuleException("Comment cannot be empty.");

        if (string.IsNullOrWhiteSpace(body) && uploads.Count > 0)
            body = uploads.Count == 1 ? $"Uploaded attachment: {uploads[0].FileName}" : $"Uploaded {uploads.Count} attachments.";

        var now = clock.UtcNow;
        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            AuthorId = currentUser.UserId,
            Visibility = isInternal ? "internal" : "public",
            CommentType = isInternal ? "note" : "reply",
            Body = body,
            CreatedAtUtc = now
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync(ct);

        foreach (var upload in uploads.Take(10))
        {
            var safeName = Path.GetFileName(string.IsNullOrWhiteSpace(upload.FileName) ? "attachment.bin" : upload.FileName);
            var contentType = string.IsNullOrWhiteSpace(upload.ContentType) ? "application/octet-stream" : upload.ContentType.Trim();
            string storedName;
            string sha256Hex;
            using (var stream = upload.OpenReadStream())
            {
                // compute hash while copying to temp buffer-less by saving via storage; to hash, copy to MemoryStream if small else temp file
                await using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                ms.Position = 0;
                var hash = await SHA256.HashDataAsync(ms, ct);
                sha256Hex = Convert.ToHexString(hash).ToLowerInvariant();
                ms.Position = 0;
                storedName = await attachmentStorage.SaveAsync(safeName, contentType, ms, ct);
            }

            db.TicketAttachments.Add(new TicketAttachment
            {
                TicketId = ticket.Id,
                CommentId = comment.Id,
                UploadedById = currentUser.UserId,
                OriginalFileName = safeName,
                StoredFileName = storedName,
                ContentType = contentType.Length > 150 ? contentType[..150] : contentType,
                FileSizeBytes = upload.Length,
                Sha256 = sha256Hex,
                IsInternal = isInternal,
                CreatedAtUtc = now
            });
        }

        db.TicketActivityLogs.Add(new TicketActivityLog
        {
            TicketId = ticket.Id,
            ActorId = currentUser.UserId,
            ActorName = currentUser.UserName,
            EventType = isInternal ? "internal_note" : "comment",
            NewValue = uploads.Count > 0 ? $"{(isInternal ? "Internal note" : "Reply")} with {uploads.Count} attachment(s)" : (isInternal ? "Internal note added" : "Reply added"),
            CreatedAtUtc = now
        });

        // First response heuristic: first public reply by non-requester while unset
        if (!isInternal && ticket.FirstResponseAtUtc is null && ticket.RequesterId != currentUser.UserId)
        {
            ticket.FirstResponseAtUtc = now;
            await SyncResponseSlaOnFirstResponseAsync(ticket.Id, now, ct);
        }

        // If requester replies on pending requester ticket, move to in progress
        if (!isInternal && ticket.RequesterId == currentUser.UserId)
        {
            var inProgress = await db.TicketStatuses.FirstOrDefaultAsync(s => s.Code == "InProgress", ct);
            var pendingRequester = await db.TicketStatuses.FirstOrDefaultAsync(s => s.Code == "PendingRequester", ct);
            if (inProgress is not null && pendingRequester is not null && ticket.StatusId == pendingRequester.Id)
            {
                var old = ticket.Status.Name;
                ticket.StatusId = inProgress.Id;
                ticket.Status = inProgress;
                db.TicketActivityLogs.Add(new TicketActivityLog
                {
                    TicketId = ticket.Id,
                    ActorId = currentUser.UserId,
                    ActorName = currentUser.UserName,
                    EventType = "status_changed",
                    FieldName = "status",
                    OldValue = old,
                    NewValue = inProgress.Name,
                    CreatedAtUtc = now
                });
                await SyncResolutionSlasOnStatusChangeAsync(ticket.Id, pendingRequester, inProgress, now, ct);
            }
        }

        ticket.UpdatedAtUtc = now;
        ticket.Version += 1;

        var commentTitle = isInternal ? $"Internal note on {ticket.TicketNumber}" : $"New reply on {ticket.TicketNumber}";
        var commentBody = body.Length > 180 ? body[..180] + "…" : body;
        await NotifyTicketParticipantsAsync(ticket, isInternal ? "ticket.internal_note" : "ticket.comment", commentTitle, commentBody, includeRequester: !isInternal, includeAssignees: true, includeWatchers: true, ct);

        await db.SaveChangesAsync(ct);
        return comment.Id;
    }

    public async Task SetWatchingAsync(long ticketId, bool watch, CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) throw new UnauthorizedAccessException();
        var scope = await accessScope.GetScopeAsync(ct);
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct)
            ?? throw new NotFoundException(nameof(Ticket), ticketId);
        if (!accessScope.CanView(ticket, scope)) throw new ForbiddenAccessException("You do not have access to this ticket.");

        var userId = currentUser.UserId.Value;
        var existing = await db.TicketWatchers.FirstOrDefaultAsync(w => w.TicketId == ticketId && w.UserId == userId, ct);
        var now = clock.UtcNow;

        if (watch)
        {
            if (existing is null)
            {
                db.TicketWatchers.Add(new TicketWatcher { TicketId = ticketId, UserId = userId, AddedAtUtc = now });
                db.TicketActivityLogs.Add(new TicketActivityLog
                {
                    TicketId = ticketId,
                    ActorId = userId,
                    ActorName = currentUser.UserName,
                    EventType = "watcher_added",
                    NewValue = currentUser.UserName,
                    CreatedAtUtc = now
                });
            }
        }
        else if (existing is not null)
        {
            db.TicketWatchers.Remove(existing);
            db.TicketActivityLogs.Add(new TicketActivityLog
            {
                TicketId = ticketId,
                ActorId = userId,
                ActorName = currentUser.UserName,
                EventType = "watcher_removed",
                NewValue = currentUser.UserName,
                CreatedAtUtc = now
            });
        }

        ticket.UpdatedAtUtc = now;
        ticket.Version += 1;
        await db.SaveChangesAsync(ct);
    }

    public async Task<TicketAttachment?> GetAttachmentForDownloadAsync(long ticketId, long attachmentId, CancellationToken ct = default)
    {
        var scope = await accessScope.GetScopeAsync(ct);
        if (!scope.IsAuthenticated) throw new UnauthorizedAccessException();

        var ticket = await db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, ct);
        if (ticket is null) return null;
        if (!accessScope.CanView(ticket, scope)) throw new ForbiddenAccessException("You do not have access to this ticket.");

        var attachment = await db.TicketAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TicketId == ticketId, ct);
        if (attachment is null) return null;
        if (attachment.IsInternal && !accessScope.CanViewInternalNotes(scope)) return null;
        return attachment;
    }

    public async Task ChangeStatusAsync(ChangeStatusRequest request, CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) throw new UnauthorizedAccessException();
        if (!currentUser.HasPermission(Permissions.Tickets.Workflow)) throw new ForbiddenAccessException("You do not have permission to change ticket status.");

        var scope = await accessScope.GetScopeAsync(ct);
        var ticket = await db.Tickets.Include(t => t.Status).FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);
        if (!accessScope.CanView(ticket, scope)) throw new ForbiddenAccessException("You do not have access to this ticket.");

        var newStatus = await db.TicketStatuses.FirstOrDefaultAsync(s => s.Id == request.NewStatusId, ct)
            ?? throw new BusinessRuleException("Invalid status.");

        if (ticket.StatusId == newStatus.Id) return;

        var now = clock.UtcNow;
        var oldStatus = ticket.Status;

        // Basic role/workflow guards (simplified)
        if (ticket.RequesterId == currentUser.UserId && !currentUser.IsInRole(AppRoleNames.Admin) && !currentUser.IsInRole(AppRoleNames.Agent) && !currentUser.IsInRole(AppRoleNames.TeamLead))
        {
            var allowedForRequester = new[] { "Closed", "Reopened", "Canceled" };
            if (!allowedForRequester.Contains(newStatus.Code, StringComparer.OrdinalIgnoreCase))
                throw new ForbiddenAccessException("You cannot move the ticket to this status.");
            if (string.Equals(newStatus.Code, "Canceled", StringComparison.OrdinalIgnoreCase) && oldStatus.SortOrder > 20)
                throw new BusinessRuleException("You can only cancel a ticket before work starts.");
        }

        if (string.Equals(newStatus.Code, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            var summary = (request.ResolutionSummary ?? request.Note ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(summary))
                throw new BusinessRuleException("A resolution note/summary is required when resolving a ticket.");
            ticket.ResolutionSummary = summary;
            ticket.ResolutionCode = request.ResolutionCode;
            ticket.ResolvedAtUtc = now;
            ticket.ResolvedById = currentUser.UserId;
            db.TicketComments.Add(new TicketComment
            {
                TicketId = ticket.Id,
                AuthorId = currentUser.UserId,
                Visibility = "public",
                CommentType = "resolution",
                Body = summary,
                CreatedAtUtc = now
            });
        }

        if (string.Equals(newStatus.Code, "Closed", StringComparison.OrdinalIgnoreCase))
        {
            ticket.ClosedAtUtc = now;
            ticket.ClosedById = currentUser.UserId;
        }

        if (string.Equals(newStatus.Code, "Canceled", StringComparison.OrdinalIgnoreCase))
        {
            ticket.CanceledAtUtc = now;
            ticket.CanceledById = currentUser.UserId;
            ticket.CancelReason = request.Note;
        }

        if (string.Equals(newStatus.Code, "Reopened", StringComparison.OrdinalIgnoreCase))
        {
            ticket.LastReopenedAtUtc = now;
            ticket.ClosedAtUtc = null;
            ticket.ResolvedAtUtc = null;
        }

        var firstResponseSetNow = false;
        if (string.Equals(newStatus.Code, "InProgress", StringComparison.OrdinalIgnoreCase) && ticket.FirstResponseAtUtc is null)
        {
            ticket.FirstResponseAtUtc = now;
            firstResponseSetNow = true;
        }

        ticket.StatusId = newStatus.Id;
        ticket.Status = newStatus;
        ticket.UpdatedAtUtc = now;
        ticket.Version += 1;

        db.TicketActivityLogs.Add(new TicketActivityLog
        {
            TicketId = ticket.Id,
            ActorId = currentUser.UserId,
            ActorName = currentUser.UserName,
            EventType = "status_changed",
            FieldName = "status",
            OldValue = oldStatus.Name,
            NewValue = newStatus.Name,
            CreatedAtUtc = now
        });

        if (firstResponseSetNow)
        {
            await SyncResponseSlaOnFirstResponseAsync(ticket.Id, now, ct);
        }
        await SyncResolutionSlasOnStatusChangeAsync(ticket.Id, oldStatus, newStatus, now, ct);

        if (!string.IsNullOrWhiteSpace(request.Note) && !string.Equals(newStatus.Code, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            db.TicketComments.Add(new TicketComment
            {
                TicketId = ticket.Id,
                AuthorId = currentUser.UserId,
                Visibility = "public",
                CommentType = "system",
                Body = request.Note.Trim(),
                CreatedAtUtc = now
            });
        }

        await NotifyTicketParticipantsAsync(ticket, "ticket.status_changed", $"Ticket {ticket.TicketNumber} status updated", $"{oldStatus.Name} → {newStatus.Name}", includeRequester: true, includeAssignees: true, includeWatchers: true, ct);

        await db.SaveChangesAsync(ct);
    }

    public async Task AssignAsync(AssignTicketRequest request, CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) throw new UnauthorizedAccessException();
        var canAssign = currentUser.HasPermission(Permissions.Tickets.Assign) || currentUser.HasPermission(Permissions.Tickets.AssignSelf);
        if (!canAssign) throw new ForbiddenAccessException("You do not have permission to assign tickets.");

        var scope = await accessScope.GetScopeAsync(ct);
        var ticket = await db.Tickets.Include(t => t.Status).Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);
        if (!accessScope.CanView(ticket, scope)) throw new ForbiddenAccessException("You do not have access to this ticket.");

        var now = clock.UtcNow;
        if (request.TeamId.HasValue)
        {
            ticket.AssignedTeamId = request.TeamId.Value;
        }

        if (request.AssigneeUserId.HasValue)
        {
            if (!currentUser.HasPermission(Permissions.Tickets.Assign) && request.AssigneeUserId.Value != currentUser.UserId.Value)
                throw new ForbiddenAccessException("You can only assign tickets to yourself.");

            var existingActive = ticket.Assignees.FirstOrDefault(a => a.UserId == request.AssigneeUserId.Value && a.UnassignedAtUtc == null);
            if (existingActive is null)
            {
                existingActive = new TicketAssignee
                {
                    TicketId = ticket.Id,
                    UserId = request.AssigneeUserId.Value,
                    IsPrimary = false,
                    AssignedAtUtc = now
                };
                db.TicketAssignees.Add(existingActive);
            }

            if (request.MakePrimary)
            {
                foreach (var a in ticket.Assignees.Where(a => a.UnassignedAtUtc == null))
                {
                    a.IsPrimary = a.UserId == request.AssigneeUserId.Value;
                }
                existingActive.IsPrimary = true;
                ticket.PrimaryAssigneeId = request.AssigneeUserId.Value;
            }

            // Auto-open if new
            var open = await db.TicketStatuses.FirstOrDefaultAsync(s => s.Code == "Open", ct);
            var inProgress = await db.TicketStatuses.FirstOrDefaultAsync(s => s.Code == "InProgress", ct);
            if (ticket.Status.Code == "New" && open is not null)
            {
                var oldStatus = ticket.Status;
                var old = oldStatus.Name;
                ticket.StatusId = open.Id;
                ticket.Status = open;
                db.TicketActivityLogs.Add(new TicketActivityLog
                {
                    TicketId = ticket.Id,
                    ActorId = currentUser.UserId,
                    ActorName = currentUser.UserName,
                    EventType = "status_changed",
                    FieldName = "status",
                    OldValue = old,
                    NewValue = open.Name,
                    CreatedAtUtc = now
                });
                await SyncResolutionSlasOnStatusChangeAsync(ticket.Id, oldStatus, open, now, ct);
            }
            else if (request.AssigneeUserId.Value == currentUser.UserId.Value && ticket.Status.Code is "New" or "Open" && inProgress is not null && request.MakePrimary)
            {
                // Accepting own assignment moves to in progress
                var oldStatus = ticket.Status;
                var old = oldStatus.Name;
                ticket.StatusId = inProgress.Id;
                ticket.Status = inProgress;
                var setFirstResponse = ticket.FirstResponseAtUtc is null;
                ticket.FirstResponseAtUtc ??= now;
                db.TicketActivityLogs.Add(new TicketActivityLog
                {
                    TicketId = ticket.Id,
                    ActorId = currentUser.UserId,
                    ActorName = currentUser.UserName,
                    EventType = "status_changed",
                    FieldName = "status",
                    OldValue = old,
                    NewValue = inProgress.Name,
                    CreatedAtUtc = now
                });
                if (setFirstResponse)
                {
                    await SyncResponseSlaOnFirstResponseAsync(ticket.Id, ticket.FirstResponseAtUtc.Value, ct);
                }
                await SyncResolutionSlasOnStatusChangeAsync(ticket.Id, oldStatus, inProgress, now, ct);
            }

            var assigneeName = await db.Users.Where(u => u.Id == request.AssigneeUserId.Value).Select(u => u.FullName).FirstOrDefaultAsync(ct);
            db.TicketActivityLogs.Add(new TicketActivityLog
            {
                TicketId = ticket.Id,
                ActorId = currentUser.UserId,
                ActorName = currentUser.UserName,
                EventType = "assigned",
                FieldName = "assignee",
                NewValue = assigneeName ?? request.AssigneeUserId.Value.ToString(),
                CreatedAtUtc = now
            });
        }

        ticket.UpdatedAtUtc = now;
        ticket.Version += 1;

        var assignBody = request.AssigneeUserId.HasValue ? "Ticket assignment was updated." : "Ticket team/assignment was updated.";
        await NotifyTicketParticipantsAsync(ticket, "ticket.assigned", $"Ticket {ticket.TicketNumber} assignment updated", assignBody, includeRequester: true, includeAssignees: true, includeWatchers: true, ct);

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TicketStatus>> GetStatusesAsync(CancellationToken ct = default) =>
        await db.TicketStatuses.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(ct);

    public async Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync(CancellationToken ct = default) =>
        await db.TicketPriorities.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync(ct);

    public async Task<IReadOnlyList<TicketCategory>> GetCategoriesAsync(CancellationToken ct = default) =>
        await db.TicketCategories.AsNoTracking().Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Team>> GetAssignableTeamsAsync(CancellationToken ct = default) =>
        await db.Teams.AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<User>> GetAssignableUsersAsync(int? teamId, CancellationToken ct = default)
    {
        var q = db.Users.AsNoTracking().Where(u => u.IsActive);
        if (teamId.HasValue)
        {
            q = q.Where(u => u.DepartmentId == teamId.Value || u.TeamMemberships.Any(tm => tm.TeamId == teamId.Value && tm.LeftAtUtc == null));
        }
        // Prefer users with Agent/TeamLead/Admin roles in general; keep simple by returning active users
        return await q.OrderBy(u => u.FullName).Take(200).ToListAsync(ct);
    }

    private async Task SyncResponseSlaOnFirstResponseAsync(long ticketId, DateTime atUtc, CancellationToken ct)
    {
        var slas = await db.TicketSlas.Where(s => s.TicketId == ticketId && s.Metric == "response").ToListAsync(ct);
        foreach (var sla in slas)
        {
            if (sla.Stage is "completed" or "canceled") continue;
            if (sla.Stage == "paused" && sla.PausedAtUtc.HasValue)
            {
                var pausedMinutes = (int)Math.Max(0, Math.Round((atUtc - sla.PausedAtUtc.Value).TotalMinutes));
                sla.TotalPausedMinutes += pausedMinutes;
                sla.DueAtUtc = sla.DueAtUtc.AddMinutes(pausedMinutes);
                sla.PausedAtUtc = null;
            }
            sla.CompletedAtUtc = atUtc;
            sla.Stage = "completed";
            sla.IsBreached = atUtc > sla.DueAtUtc;
            if (sla.IsBreached) sla.BreachedAtUtc ??= atUtc;
        }
    }

    private async Task SyncResolutionSlasOnStatusChangeAsync(long ticketId, TicketStatus oldStatus, TicketStatus newStatus, DateTime atUtc, CancellationToken ct)
    {
        var slas = await db.TicketSlas.Where(s => s.TicketId == ticketId && s.Metric == "resolution").ToListAsync(ct);
        if (slas.Count == 0) return;

        var newPauses = newStatus.PausesSla;
        var newTerminal = newStatus.IsTerminal;
        var newResolvedLike = string.Equals(newStatus.Code, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(newStatus.Code, "Closed", StringComparison.OrdinalIgnoreCase);
        var newCanceled = string.Equals(newStatus.Code, "Canceled", StringComparison.OrdinalIgnoreCase);

        foreach (var sla in slas)
        {
            if (sla.Stage == "canceled") continue;

            if (newCanceled)
            {
                if (sla.Stage == "paused" && sla.PausedAtUtc.HasValue)
                {
                    var pausedMinutes = (int)Math.Max(0, Math.Round((atUtc - sla.PausedAtUtc.Value).TotalMinutes));
                    sla.TotalPausedMinutes += pausedMinutes;
                    sla.DueAtUtc = sla.DueAtUtc.AddMinutes(pausedMinutes);
                    sla.PausedAtUtc = null;
                }
                sla.Stage = "canceled";
                sla.CompletedAtUtc ??= atUtc;
                continue;
            }

            if (newResolvedLike || (newTerminal && !string.Equals(newStatus.Code, "Canceled", StringComparison.OrdinalIgnoreCase)))
            {
                if (sla.Stage == "paused" && sla.PausedAtUtc.HasValue)
                {
                    var pausedMinutes = (int)Math.Max(0, Math.Round((atUtc - sla.PausedAtUtc.Value).TotalMinutes));
                    sla.TotalPausedMinutes += pausedMinutes;
                    sla.DueAtUtc = sla.DueAtUtc.AddMinutes(pausedMinutes);
                    sla.PausedAtUtc = null;
                }
                if (sla.Stage != "completed")
                {
                    sla.Stage = "completed";
                    sla.CompletedAtUtc = atUtc;
                }
                sla.IsBreached = (sla.CompletedAtUtc ?? atUtc) > sla.DueAtUtc;
                if (sla.IsBreached) sla.BreachedAtUtc ??= sla.CompletedAtUtc ?? atUtc;
                continue;
            }

            if (newPauses)
            {
                if (sla.Stage == "running")
                {
                    sla.Stage = "paused";
                    sla.PausedAtUtc = atUtc;
                }
                continue;
            }

            // Unpaused/open active states (New/Open/InProgress/Reopened/Pending? no, pending pause) -> resume if paused
            if (sla.Stage == "paused" && sla.PausedAtUtc.HasValue)
            {
                var pausedMinutes = (int)Math.Max(0, Math.Round((atUtc - sla.PausedAtUtc.Value).TotalMinutes));
                sla.TotalPausedMinutes += pausedMinutes;
                sla.DueAtUtc = sla.DueAtUtc.AddMinutes(pausedMinutes);
                sla.PausedAtUtc = null;
                sla.Stage = "running";
            }
            else if (sla.Stage == "completed")
            {
                // Reopened/reactivated after completion -> restart tracking from now (simple behavior)
                sla.Stage = "running";
                sla.CompletedAtUtc = null;
                sla.IsBreached = atUtc > sla.DueAtUtc;
                if (!sla.IsBreached) sla.BreachedAtUtc = null;
            }
            else if (sla.Stage != "running")
            {
                sla.Stage = "running";
            }
        }

        // Keep ticket-level due fields in sync with active resolution SLA for convenience
        var activeResolution = slas
            .Where(s => s.Stage is "running" or "paused")
            .OrderBy(s => s.DueAtUtc)
            .FirstOrDefault() ?? slas.OrderByDescending(s => s.DueAtUtc).FirstOrDefault();
        if (activeResolution is not null)
        {
            var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket is not null)
            {
                ticket.ResolutionDueAtUtc = activeResolution.DueAtUtc;
            }
        }
    }

    private async Task NotifyTicketParticipantsAsync(Ticket ticket, string type, string title, string? body, bool includeRequester, bool includeAssignees, bool includeWatchers, CancellationToken ct)
    {
        var actorId = currentUser.UserId;
        var recipientIds = new HashSet<long>();
        if (includeRequester && ticket.RequesterId.HasValue && ticket.RequesterId.Value > 0) recipientIds.Add(ticket.RequesterId.Value);
        if (includeAssignees)
        {
            if (ticket.PrimaryAssigneeId.HasValue) recipientIds.Add(ticket.PrimaryAssigneeId.Value);
            var assignees = await db.TicketAssignees.AsNoTracking()
                .Where(a => a.TicketId == ticket.Id && a.UnassignedAtUtc == null)
                .Select(a => a.UserId)
                .ToListAsync(ct);
            foreach (var id in assignees) recipientIds.Add(id);
        }
        if (includeWatchers)
        {
            var watchers = await db.TicketWatchers.AsNoTracking()
                .Where(w => w.TicketId == ticket.Id)
                .Select(w => w.UserId)
                .ToListAsync(ct);
            foreach (var id in watchers) recipientIds.Add(id);
        }

        if (actorId.HasValue) recipientIds.Remove(actorId.Value);
        if (recipientIds.Count == 0) return;

        var now = clock.UtcNow;
        var link = $"/tickets/{ticket.Id}";
        foreach (var rid in recipientIds)
        {
            db.Notifications.Add(new Notification
            {
                RecipientId = rid,
                TicketId = ticket.Id,
                Type = type,
                Title = title,
                Body = body,
                LinkUrl = link,
                IsRead = false,
                CreatedAtUtc = now
            });
        }
    }
}
