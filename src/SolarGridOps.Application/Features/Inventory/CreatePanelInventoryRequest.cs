namespace SolarGridOps.Application.Features.Inventory;

public class CreatePanelInventoryRequest
{
    public string SerialNumber { get; set; } = string.Empty;
    public int Wattage { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Notes { get; set; }
}
