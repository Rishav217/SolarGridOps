namespace SolarGridOps.Domain.Entities;

using SolarGridOps.Domain.Enums;

public class InstallationSession : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? TechnicianUserId { get; set; }
    public DateTime SessionDateUtc { get; set; } = DateTime.UtcNow;
    public string? WorkSummary { get; set; }
    public bool IsCompletedForDay { get; set; } = false;
    public InstallationClosureStatus ClosureStatus { get; set; } = InstallationClosureStatus.Open;
    public DateTime? ClosureRequestedAtUtc { get; set; }
    public DateTime? ClosureApprovedAtUtc { get; set; }
    public Guid? ClosureRequestedByUserId { get; set; }
    public Guid? ClosureApprovedByUserId { get; set; }
    public string? CustomerSignatureName { get; set; }
    public string? CustomerSignatureBase64 { get; set; }
    public string? ClosureNotes { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<InstallationEvidence> EvidenceItems { get; set; } = new List<InstallationEvidence>();
}