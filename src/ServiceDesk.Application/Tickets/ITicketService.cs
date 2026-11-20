using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Application.Tickets;

public interface ITicketService
{
    Task<(IReadOnlyList<TicketListItemDto> Items, int TotalCount)> GetTicketsAsync(TicketQuery query, CancellationToken ct = default);
    Task<TicketDetailsDto?> GetDetailsAsync(long id, CancellationToken ct = default);
    Task<TicketDetailsDto?> GetDetailsByNumberAsync(string ticketNumber, CancellationToken ct = default);
    Task<Ticket> CreateAsync(CreateTicketRequest request, CancellationToken ct = default);
    Task<long> AddCommentAsync(AddCommentRequest request, CancellationToken ct = default);
    Task ChangeStatusAsync(ChangeStatusRequest request, CancellationToken ct = default);
    Task AssignAsync(AssignTicketRequest request, CancellationToken ct = default);
    Task SetWatchingAsync(long ticketId, bool watch, CancellationToken ct = default);
    Task<TicketAttachment?> GetAttachmentForDownloadAsync(long ticketId, long attachmentId, CancellationToken ct = default);
    Task<IReadOnlyList<TicketStatus>> GetStatusesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TicketCategory>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Team>> GetAssignableTeamsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAssignableUsersAsync(int? teamId, CancellationToken ct = default);
}

public sealed record TicketQuery(
    string? Search = null,
    short? StatusId = null,
    short? PriorityId = null,
    int? TeamId = null,
    bool? OpenOnly = null,
    int Page = 1,
    int PageSize = 20,
    string Sort = "updated_desc"
);
