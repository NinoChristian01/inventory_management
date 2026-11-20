namespace ServiceDesk.Domain.Entities;

public class Notification
{
    public long Id { get; set; }
    public long RecipientId { get; set; }
    public long? TicketId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }

    public User Recipient { get; set; } = null!;
    public Ticket? Ticket { get; set; }
}