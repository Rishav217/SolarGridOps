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

        var requiredPermissions = new (string Key, string Name)[]
        {
            ("customers.read", "Read Customers"),
            ("customers.create", "Create Customers"),
            ("projects.read", "Read Projects"),
            ("projects.create", "Create Projects"),
            ("projects.updatePhase", "Update Project Phase"),
            ("installations.sessions.read", "Read Installation Sessions"),
            ("installations.sessions.create", "Create Installation Sessions"),
            ("installations.sessions.update", "Update Installation Sessions"),
            ("installations.evidence.read", "Read Installation Evidence"),
            ("installations.evidence.create", "Create Installation Evidence"),
            ("inventory.panels.read", "Read Inventory Panels"),
            ("inventory.panels.create", "Create Inventory Panels"),
            ("inventory.panels.update", "Update Inventory Panels"),
            ("inventory.panels.delete", "Delete Inventory Panels"),
            ("auth.capabilities", "Read Capabilities")
        };

        var existingPermissions = await db.Permissions
            .ToDictionaryAsync(x => x.PermissionKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var permission in requiredPermissions)
        {
            if (!existingPermissions.ContainsKey(permission.Key))
            {
                var entity = new Permission
                {
                    PermissionKey = permission.Key,
                    DisplayName = permission.Name
                };
                db.Permissions.Add(entity);
                existingPermissions[permission.Key] = entity;
            }
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
            db.Roles.Add(role);
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.MobileNumber == "9999999999", cancellationToken)
            ?? await db.Users.FirstOrDefaultAsync(x => x.Email == "owner@solargridops.local", cancellationToken);

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
            db.Users.Add(user);
        }
        else if (!user.IsActive)
        {
            user.IsActive = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        var rolePermissionIds = await db.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in existingPermissions.Values)
        {
            if (!rolePermissionIds.Contains(permission.Id))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        var hasUserRole = await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, cancellationToken);
        if (!hasUserRole)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
