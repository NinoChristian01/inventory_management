using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.Domain.Entities;

/// <summary>
/// Application user. Imported employees start inactive with no password; an admin activates them
/// and issues a one-time temporary password that must be changed on first sign-in.
/// </summary>
public class User : IdentityUser<long>
{
    public string? EmployeeNumber { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Suffix { get; set; }

    /// <summary>Composed display name (kept in sync by application logic).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Department/office of the user (team as department).</summary>
    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public long? LegacyEmployeeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Team? Department { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<UserClaim> Claims { get; set; } = [];
    public ICollection<UserLogin> Logins { get; set; } = [];
    public ICollection<UserToken> Tokens { get; set; } = [];
    public ICollection<TeamMember> TeamMemberships { get; set; } = [];
}