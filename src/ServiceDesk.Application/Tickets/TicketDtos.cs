using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Application.Tickets;

public sealed record TicketListItemDto(
    long Id,
    string TicketNumber,
    string Subject,
    string Status,
    string StatusCode,
    string Priority,
    string PriorityCode,
    string? Category,
    string Requester,
    string? Assignee,
    string? Team,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? DueAtUtc,
    bool IsOpen
);

public sealed record CreateTicketRequest(
    string Subject,
    string Description,
    short PriorityId,
    int? CategoryId,
    int? AssignedTeamId,
    long? RequesterId = null
);

public sealed record AttachmentUpload(
    string FileName,
    string ContentType,
    long Length,
    Func<Stream> OpenReadStream
);

public sealed record AddCommentRequest(
    long TicketId,
    string Body,
    bool IsInternal,
    IReadOnlyList<AttachmentUpload>? Attachments = null
);

public sealed record ChangeStatusRequest(
    long TicketId,
    short NewStatusId,
    string? Note,
    string? ResolutionCode = null,
    string? ResolutionSummary = null
);

public sealed record AssignTicketRequest(
    long TicketId,
    long? AssigneeUserId,
    int? TeamId,
    bool MakePrimary = true
);

public sealed record TicketAttachmentDownload(
    long Id,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    long FileSizeBytes
);

public sealed record TicketDetailsDto(
    Ticket Ticket,
    IReadOnlyList<TicketComment> Comments,
    IReadOnlyList<TicketActivityLog> Activity,
    IReadOnlyList<TicketAssignee> Assignees,
    IReadOnlyList<TicketAttachment> Attachments,
    IReadOnlyList<TicketWatcher> Watchers
);
