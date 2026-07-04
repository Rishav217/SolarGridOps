namespace SolarGridOps.Application.Features.Installations;

public class InstallationEvidenceDto
{
    public Guid Id { get; set; }
    public Guid InstallationSessionId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public DateTime CapturedAtUtc { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Notes { get; set; }
}
