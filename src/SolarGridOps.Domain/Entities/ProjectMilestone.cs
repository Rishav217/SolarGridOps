using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Domain.Entities;

public class ProjectMilestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public ProjectPhase Phase { get; set; }
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}