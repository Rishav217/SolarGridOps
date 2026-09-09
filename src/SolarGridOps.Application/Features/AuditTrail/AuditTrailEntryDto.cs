namespace SolarGridOps.Application.Features.AuditTrail;

public class AuditTrailEntryDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string ActionKey { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = "System";
}
