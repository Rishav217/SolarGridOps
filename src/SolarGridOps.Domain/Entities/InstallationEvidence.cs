namespace SolarGridOps.Domain.Entities;

public class InstallationEvidence : BaseEntity
{
    public Guid InstallationSessionId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Photo";
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Notes { get; set; }

    public InstallationSession InstallationSession { get; set; } = null!;
}