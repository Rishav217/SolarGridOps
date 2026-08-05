namespace SolarGridOps.Application.Features.Inventory;

public class CreateInverterInventoryRequest
{
    public string SerialNumber { get; set; } = string.Empty;
    public decimal CapacityKva { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Notes { get; set; }
}
