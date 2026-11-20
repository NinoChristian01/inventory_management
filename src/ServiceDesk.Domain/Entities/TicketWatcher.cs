namespace ServiceDesk.Domain.Entities;

public class TicketWatcher
{
    public long TicketId { get; set; }
    public long UserId { get; set; }
    public DateTime AddedAtUtc { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
}