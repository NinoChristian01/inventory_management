using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.Domain.Entities;

public class UserLogin : IdentityUserLogin<long>
{
    public virtual User User { get; set; } = null!;
}