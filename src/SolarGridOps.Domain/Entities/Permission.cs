namespace SolarGridOps.Domain.Entities;

public class Permission : BaseEntity
{
    public string PermissionKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
