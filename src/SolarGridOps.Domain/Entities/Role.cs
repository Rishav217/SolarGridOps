using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Domain.Entities;

public class Role : BaseEntity
{
    public string RoleKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
