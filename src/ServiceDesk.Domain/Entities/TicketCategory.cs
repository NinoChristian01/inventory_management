namespace ServiceDesk.Domain.Entities;

public class TicketCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int? DefaultTeamId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public TicketCategory? Parent { get; set; }
    public ICollection<TicketCategory> Children { get; set; } = [];
    public Team? DefaultTeam { get; set; }
}