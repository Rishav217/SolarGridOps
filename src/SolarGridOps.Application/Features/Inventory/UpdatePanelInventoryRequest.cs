namespace SolarGridOps.Application.Features.Inventory;

public class UpdatePanelInventoryRequest
{
    public string? SerialNumber { get; set; }
    public int? Wattage { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Notes { get; set; }
}
