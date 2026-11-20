namespace ServiceDesk.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public long? ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public short ActivityType { get; set; }
    public string? TableName { get; set; }
    public long? RecordId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public DateTime CreatedAtUtc { get; set; }
}