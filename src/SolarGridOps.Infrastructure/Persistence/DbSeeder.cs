using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Domain.Entities;
using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        var requiredPermissions = new Dictionary<string, string>
        {
            ["customers.read"] = "Read Customers",
            ["customers.create"] = "Create Customers",
            ["auth.capabilities"] = "Read Capabilities",
            ["auth.logout"] = "Logout"
        };

        var existingPermissions = await db.Permissions
            .Where(x => requiredPermissions.Keys.Contains(x.PermissionKey))
            .ToDictionaryAsync(x => x.PermissionKey, cancellationToken);

        foreach (var item in requiredPermissions)
        {
            if (existingPermissions.ContainsKey(item.Key))
            {
                continue;
            }

            var permission = new Permission
            {
                PermissionKey = item.Key,
                DisplayName = item.Value
            };

            await db.Permissions.AddAsync(permission, cancellationToken);
            existingPermissions[item.Key] = permission;
        }

        var role = await db.Roles.FirstOrDefaultAsync(x => x.RoleKey == "owner_admin", cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                RoleKey = "owner_admin",
                DisplayName = "Owner Admin",
                RoleType = RoleType.OwnerAdmin
            };

            await db.Roles.AddAsync(role, cancellationToken);
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.MobileNumber == "9999999999", cancellationToken);
        if (user is null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            user = new User
            {
                FullName = "Owner Admin",
                MobileNumber = "9999999999",
                Email = "owner@solargridops.local",
                PasswordHash = passwordHash,
                IsActive = true
            };

            await db.Users.AddAsync(user, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        var existingRolePermissionIds = await db.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in existingPermissions.Values)
        {
            if (existingRolePermissionIds.Contains(permission.Id))
            {
                continue;
            }

            await db.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            }, cancellationToken);
        }

        var hasUserRole = await db.UserRoles
            .AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, cancellationToken);
        if (!hasUserRole)
        {
            await db.UserRoles.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            }, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
