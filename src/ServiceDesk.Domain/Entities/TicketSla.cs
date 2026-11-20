namespace ServiceDesk.Domain.Entities;

public class TicketSla
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public int SlaPolicyId { get; set; }
    public string Metric { get; set; } = string.Empty; // response or resolution
    public DateTime StartedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }
    public DateTime? PausedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string Stage { get; set; } = "running"; // running, paused, completed, canceled
    public bool IsBreached { get; set; }
    public DateTime? BreachedAtUtc { get; set; }
    public bool WarningSent { get; set; }
    public bool BreachSent { get; set; }
    public int TotalPausedMinutes { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public SlaPolicy SlaPolicy { get; set; } = null!;
}