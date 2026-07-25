using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Domain.Entities;

public class CustomerDocument : BaseEntity
{
    public Guid CustomerId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public int Version { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}