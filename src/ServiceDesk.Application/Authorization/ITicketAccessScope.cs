using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Application.Authorization;

/// <summary>
/// Resolves the current user's access scope for ticket queries and checks.
/// Every ticket list/search/export/API query must be filtered through this scope.
/// </summary>
public interface ITicketAccessScope
{
    Task<TicketAccessScopeResult> GetScopeAsync(CancellationToken cancellationToken = default);
    IQueryable<Ticket> Apply(IQueryable<Ticket> query, TicketAccessScopeResult scope);
    bool CanView(Ticket ticket, TicketAccessScopeResult scope);
    bool CanViewInternalNotes(TicketAccessScopeResult scope);
}

public sealed record TicketAccessScopeResult(
    long UserId,
    bool IsAuthenticated,
    bool CanViewAll,
    bool CanViewTeam,
    bool CanViewInternalNotes,
    IReadOnlyCollection<int> TeamIds,
    IReadOnlyCollection<long> WatchedTicketIds
);
