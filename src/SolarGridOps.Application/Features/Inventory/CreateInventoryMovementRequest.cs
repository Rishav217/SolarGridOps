namespace SolarGridOps.Application.Features.Inventory;

public class CreateInventoryMovementRequest
{
    public Guid ProjectId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? UnitCostPrice { get; set; }
    public decimal? UnitSellPrice { get; set; }
    public string? Notes { get; set; }
}
