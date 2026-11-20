namespace ServiceDesk.Domain.Entities;

public class TicketStatus
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public bool IsTerminal { get; set; }
    public bool PausesSla { get; set; }
    public byte SortOrder { get; set; }
}