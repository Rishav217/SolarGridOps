namespace SolarGridOps.Domain.Entities;

public class InvoiceRecord : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime InvoiceDateUtc { get; set; }
    public int Version { get; set; } = 1;
    public string? FilePath { get; set; }
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
