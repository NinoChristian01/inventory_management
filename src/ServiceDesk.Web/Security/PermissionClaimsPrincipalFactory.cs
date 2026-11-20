using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Security;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Web.Security;

public sealed class PermissionClaimsPrincipalFactory(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<IdentityOptions> optionsAccessor,
    AppDbContext dbContext)
    : UserClaimsPrincipalFactory<User, Role>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("full_name", user.FullName ?? user.UserName ?? string.Empty));
        if (user.MustChangePassword)
        {
            identity.AddClaim(new Claim("must_change_password", "true"));
        }
        if (user.DepartmentId.HasValue)
        {
            identity.AddClaim(new Claim("department_id", user.DepartmentId.Value.ToString()));
        }

        // Permission claims from roles (stored as RoleClaims with type "permission")
        var roleIds = await dbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (roleIds.Count > 0)
        {
            var permissions = await dbContext.RoleClaims
                .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType && rc.ClaimValue != null)
                .Select(rc => rc.ClaimValue!)
                .Distinct()
                .ToListAsync();

            foreach (var permission in permissions)
            {
                if (!identity.HasClaim(Permissions.ClaimType, permission))
                {
                    identity.AddClaim(new Claim(Permissions.ClaimType, permission));
                }
            }
        }

        return identity;
    }
}
