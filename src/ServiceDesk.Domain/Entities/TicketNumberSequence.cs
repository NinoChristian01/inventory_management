namespace ServiceDesk.Domain.Entities;

public class TicketNumberSequence
{
    public string ScopeKey { get; set; } = string.Empty; // e.g. MIS-26K
    public int LastValue { get; set; }
}