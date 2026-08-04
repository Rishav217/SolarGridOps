namespace SolarGridOps.Application.Features.Inventory;

public class PanelInventoryDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public int Wattage { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public string? Notes { get; set; }
}
