namespace SolarGridOps.Domain.Entities;

public class PaymentReceipt : BaseEntity
{
    public Guid ProjectId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ReceiptDateUtc { get; set; }
    public string PaymentMode { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
