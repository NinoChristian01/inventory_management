namespace ServiceDesk.Domain.Entities;

public class TicketActivityLog
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public long? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User? Actor { get; set; }
}