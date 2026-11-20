using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.Domain.Entities;

public class RoleClaim : IdentityRoleClaim<long>
{
    public virtual Role Role { get; set; } = null!;
}