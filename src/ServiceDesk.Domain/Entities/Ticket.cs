namespace ServiceDesk.Domain.Entities;

public class Ticket
{
    public long Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;

    public long? RequesterId { get; set; }
    public int? RequesterDepartmentId { get; set; }
    public string? RequesterNameSnapshot { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }

    public short StatusId { get; set; }
    public short PriorityId { get; set; }
    public int? CategoryId { get; set; }
    public int? AssignedTeamId { get; set; }
    public long? PrimaryAssigneeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? FirstResponseAtUtc { get; set; }
    public DateTime? ResponseDueAtUtc { get; set; }
    public DateTime? ResolutionDueAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public DateTime? CanceledAtUtc { get; set; }
    public DateTime? LastReopenedAtUtc { get; set; }

    public long? ResolvedById { get; set; }
    public long? ClosedById { get; set; }
    public string? CloseReason { get; set; }
    public long? CanceledById { get; set; }
    public string? CancelReason { get; set; }
    public string? ResolutionCode { get; set; }
    public string? ResolutionSummary { get; set; }

    public byte? SatisfactionRating { get; set; }
    public string? SatisfactionComment { get; set; }
    public DateTime? SatisfactionRatedAtUtc { get; set; }

    public long? LegacyRequestId { get; set; }

    // Legacy fields preserved from rqst for completeness/audit
    public long? LegacyNotedById { get; set; }
    public string? LegacyNotedByName { get; set; }
    public long? LegacyApprovedById { get; set; }
    public string? LegacyApprovedByName { get; set; }
    public int? LegacyTurnaroundValue { get; set; }
    public string? LegacyTurnaroundUnit { get; set; }
    public bool LegacyIsArchived { get; set; }
    public DateTime? LegacyArchivedAtUtc { get; set; }

    public int Version { get; set; }

    public User? Requester { get; set; }
    public Team? RequesterDepartment { get; set; }
    public TicketStatus Status { get; set; } = null!;
    public TicketPriority Priority { get; set; } = null!;
    public TicketCategory? Category { get; set; }
    public Team? AssignedTeam { get; set; }
    public User? PrimaryAssignee { get; set; }
    public User? ResolvedBy { get; set; }
    public User? ClosedBy { get; set; }
    public User? CanceledBy { get; set; }

    public ICollection<TicketAssignee> Assignees { get; set; } = [];
    public ICollection<TicketWatcher> Watchers { get; set; } = [];
    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketAttachment> Attachments { get; set; } = [];
    public ICollection<TicketActivityLog> ActivityLogs { get; set; } = [];
    public ICollection<TicketSla> Slas { get; set; } = [];
}