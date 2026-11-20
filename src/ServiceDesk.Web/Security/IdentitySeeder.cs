using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Security;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Web.Security;

public static class IdentitySeeder
{
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminPassword = "Admin@12345";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                return;
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await EnsureRolesAndPermissionsAsync(db, roleManager, cancellationToken);
            await EnsureAdminUserAsync(db, userManager, cancellationToken);
        }
        catch
        {
            // Seeding is best-effort at startup; do not block app startup.
        }
    }

    private static async Task EnsureRolesAndPermissionsAsync(AppDbContext db, RoleManager<Role> roleManager, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        foreach (var roleName in AppRoleNames.All)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new Role
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    IsSystem = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                var create = await roleManager.CreateAsync(role);
                if (!create.Succeeded || role.Id == 0)
                {
                    role = await roleManager.FindByNameAsync(roleName);
                }
            }

            if (role is null || role.Id == 0) continue;
            if (!Permissions.RoleDefaults.TryGetValue(roleName, out var defaults))
            {
                defaults = [];
            }

            var existing = await db.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue)
                .ToListAsync(ct);

            foreach (var permission in defaults.Distinct(StringComparer.Ordinal))
            {
                if (existing.Contains(permission)) continue;
                db.RoleClaims.Add(new RoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = Permissions.ClaimType,
                    ClaimValue = permission
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureAdminUserAsync(AppDbContext db, UserManager<User> userManager, CancellationToken ct)
    {
        var adminRoleId = await db.Roles
            .Where(r => r.NormalizedName == AppRoleNames.Admin.ToUpperInvariant())
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (adminRoleId != 0)
        {
            var hasAnyAdmin = await db.UserRoles.AnyAsync(ur => ur.RoleId == adminRoleId, ct);
            if (hasAnyAdmin)
            {
                return;
            }
        }

        var now = DateTime.UtcNow;
        var admin = await userManager.FindByNameAsync(DefaultAdminUserName);
        if (admin is null)
        {
            admin = new User
            {
                UserName = DefaultAdminUserName,
                Email = "admin@servicedesk.local",
                EmailConfirmed = true,
                LastName = "System",
                FirstName = "Administrator",
                FullName = "System Administrator",
                EmployeeNumber = "ADMIN",
                IsActive = true,
                MustChangePassword = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                LockoutEnabled = true
            };

            var created = await userManager.CreateAsync(admin, DefaultAdminPassword);
            if (!created.Succeeded)
            {
                // If duplicate/race, try to load existing and continue.
                admin = await userManager.FindByNameAsync(DefaultAdminUserName);
            }
        }

        if (admin is null) return;

        if (!admin.IsActive || admin.MustChangePassword)
        {
            admin.IsActive = true;
            admin.MustChangePassword = false;
            admin.UpdatedAtUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(admin);
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoleNames.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoleNames.Admin);
        }
    }
}
