namespace ServiceDesk.Domain.Entities;

public class TeamMember
{
    public int TeamId { get; set; }
    public long UserId { get; set; }
    public bool IsLead { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public DateTime? LeftAtUtc { get; set; }

    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}