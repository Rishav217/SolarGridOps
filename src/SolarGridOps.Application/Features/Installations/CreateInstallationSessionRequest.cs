namespace SolarGridOps.Application.Features.Installations;

public class CreateInstallationSessionRequest
{
    public Guid ProjectId { get; set; }
    public Guid? TechnicianUserId { get; set; }
    public DateTime? SessionDateUtc { get; set; }
    public string? WorkSummary { get; set; }
    public bool IsCompletedForDay { get; set; }
}
