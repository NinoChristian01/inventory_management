using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Authorization;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Application.Security;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Infrastructure.Authorization;

public sealed class TicketAccessScope(AppDbContext dbContext, ICurrentUser currentUser) : ITicketAccessScope
{
    public async Task<TicketAccessScopeResult> GetScopeAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        if (!currentUser.IsAuthenticated || userId is null)
        {
            return new TicketAccessScopeResult(0, false, false, false, false, [], []);
        }

        var canViewAll = currentUser.HasPermission(Permissions.Tickets.ViewAll);
        var canViewTeam = currentUser.HasPermission(Permissions.Tickets.ViewTeam) || canViewAll;
        var canViewInternal = currentUser.HasPermission(Permissions.Tickets.InternalNotes) || canViewAll;

        var teamIds = await dbContext.TeamMembers
            .Where(tm => tm.UserId == userId.Value && tm.LeftAtUtc == null)
            .Select(tm => tm.TeamId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Also include department team if present (department acts as team in this model)
        var departmentId = await dbContext.Users
            .Where(u => u.Id == userId.Value)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(cancellationToken);
        if (departmentId.HasValue && !teamIds.Contains(departmentId.Value))
        {
            teamIds.Add(departmentId.Value);
        }

        // Watched tickets: table exists; load ids (kept simple for now)
        List<long> watched = [];
        try
        {
            watched = await dbContext.TicketWatchers
                .Where(w => w.UserId == userId.Value)
                .Select(w => w.TicketId)
                .ToListAsync(cancellationToken);
        }
        catch
        {
            // Table/query may not be available in all contexts yet
        }

        return new TicketAccessScopeResult(userId.Value, true, canViewAll, canViewTeam, canViewInternal, teamIds, watched);
    }

    public IQueryable<Ticket> Apply(IQueryable<Ticket> query, TicketAccessScopeResult scope)
    {
        if (!scope.IsAuthenticated) return query.Where(_ => false);
        if (scope.CanViewAll) return query;

        var uid = scope.UserId;
        var teamIds = scope.TeamIds;
        var watched = scope.WatchedTicketIds;

        return query.Where(t =>
            t.RequesterId == uid ||
            t.PrimaryAssigneeId == uid ||
            (t.AssignedTeamId != null && teamIds.Contains(t.AssignedTeamId.Value)) ||
            t.Assignees.Any(a => a.UserId == uid && a.UnassignedAtUtc == null) ||
            watched.Contains(t.Id));
    }

    public bool CanView(Ticket ticket, TicketAccessScopeResult scope)
    {
        if (!scope.IsAuthenticated) return false;
        if (scope.CanViewAll) return true;
        if (ticket.RequesterId == scope.UserId) return true;
        if (ticket.PrimaryAssigneeId == scope.UserId) return true;
        if (ticket.AssignedTeamId.HasValue && scope.TeamIds.Contains(ticket.AssignedTeamId.Value)) return true;
        if (ticket.Assignees.Any(a => a.UserId == scope.UserId && a.UnassignedAtUtc == null)) return true;
        if (scope.WatchedTicketIds.Contains(ticket.Id)) return true;
        return false;
    }

    public bool CanViewInternalNotes(TicketAccessScopeResult scope) => scope.CanViewInternalNotes;
}
