namespace ServiceDesk.Domain.Entities;

/// <summary>
/// Office/department and/or service team. A user belongs to a department and may be a member of
/// multiple support teams.
/// </summary>
public class Team
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsServiceTeam { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public long? ManagerUserId { get; set; }
    public int? LegacyOfficeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public User? ManagerUser { get; set; }
    public ICollection<User> DepartmentUsers { get; set; } = [];
    public ICollection<TeamMember> Members { get; set; } = [];
}