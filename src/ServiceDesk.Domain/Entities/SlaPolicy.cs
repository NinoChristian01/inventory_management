namespace ServiceDesk.Domain.Entities;

public class SlaPolicy
{
    public int Id { get; set; }
    public short PriorityId { get; set; }
    public int? CategoryId { get; set; }
    public int ResponseTargetMinutes { get; set; }
    public int ResolutionTargetMinutes { get; set; }
    public bool UseBusinessHours { get; set; }
    public byte WarningPercent { get; set; } = 75;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public TicketPriority Priority { get; set; } = null!;
    public TicketCategory? Category { get; set; }
}