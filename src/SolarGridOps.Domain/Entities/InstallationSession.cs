namespace SolarGridOps.Domain.Entities;

public class InstallationSession : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? TechnicianUserId { get; set; }
    public DateTime SessionDateUtc { get; set; } = DateTime.UtcNow;
    public string? WorkSummary { get; set; }
    public bool IsCompletedForDay { get; set; } = false;

    public Project Project { get; set; } = null!;
    public ICollection<InstallationEvidence> EvidenceItems { get; set; } = new List<InstallationEvidence>();
}