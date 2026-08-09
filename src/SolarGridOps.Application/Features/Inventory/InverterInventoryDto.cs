namespace SolarGridOps.Application.Features.Inventory;

public class InverterInventoryDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public decimal CapacityKva { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public string? Notes { get; set; }
}
