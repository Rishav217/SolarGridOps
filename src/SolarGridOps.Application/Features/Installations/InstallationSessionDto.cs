namespace SolarGridOps.Application.Features.Installations;

using SolarGridOps.Domain.Enums;

public class InstallationSessionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TechnicianUserId { get; set; }
    public DateTime SessionDateUtc { get; set; }
    public string? WorkSummary { get; set; }
    public bool IsCompletedForDay { get; set; }
    public InstallationClosureStatus ClosureStatus { get; set; }
    public DateTime? ClosureRequestedAtUtc { get; set; }
    public DateTime? ClosureApprovedAtUtc { get; set; }
    public string? CustomerSignatureName { get; set; }
    public string? ClosureNotes { get; set; }
    public int EvidenceCount { get; set; }
}
