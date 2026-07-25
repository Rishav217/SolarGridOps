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

        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var permissions = new[]
        {
            new Permission { PermissionKey = "customers.read", DisplayName = "Read Customers" },
            new Permission { PermissionKey = "customers.create", DisplayName = "Create Customers" },
            new Permission { PermissionKey = "auth.capabilities", DisplayName = "Read Capabilities" }
        };

        var role = new Role
        {
            RoleKey = "owner_admin",
            DisplayName = "Owner Admin",
            RoleType = RoleType.OwnerAdmin
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var user = new User
        {
            FullName = "Owner Admin",
            MobileNumber = "9999999999",
            Email = "owner@solargridops.local",
            PasswordHash = passwordHash,
            IsActive = true
        };

        db.Permissions.AddRange(permissions);
        db.Roles.Add(role);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
