using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.Domain.Entities;

public class UserToken : IdentityUserToken<long>
{
    public virtual User User { get; set; } = null!;
}