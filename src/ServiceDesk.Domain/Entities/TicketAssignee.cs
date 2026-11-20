namespace ServiceDesk.Domain.Entities;

public class TicketAssignee
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public long UserId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public DateTime? UnassignedAtUtc { get; set; } // null means current

    public Ticket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
}