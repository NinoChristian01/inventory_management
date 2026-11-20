namespace ServiceDesk.Domain.Entities;

public class TicketPriority
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public byte SortOrder { get; set; }
}