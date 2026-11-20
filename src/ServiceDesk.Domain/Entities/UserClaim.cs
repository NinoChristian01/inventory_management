using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.Domain.Entities;

public class UserClaim : IdentityUserClaim<long>
{
    public virtual User User { get; set; } = null!;
}