namespace SolarGridOps.Application.Features.Installations;

public class AddInstallationEvidenceRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Photo";
    public DateTime? CapturedAtUtc { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Notes { get; set; }
}
