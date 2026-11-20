using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.Domain.Entities;

/// <summary>
/// Application role (Requester, Agent, TeamLead, Admin). Permissions are stored as role claims.
/// </summary>
public class Role : IdentityRole<long>
{
    public bool IsSystem { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RoleClaim> RoleClaims { get; set; } = [];
}