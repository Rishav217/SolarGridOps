namespace SolarGridOps.Domain.Entities;

public class PanelAssignment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public int Wattage { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
