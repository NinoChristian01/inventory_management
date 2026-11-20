namespace ServiceDesk.Domain.Enums;

/// <summary>
/// Ticket priorities. Each value is also the fixed primary key seeded into
/// <c>ticket_priorities</c>. Never renumber these.
/// </summary>
public enum PriorityCode : short
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
}
