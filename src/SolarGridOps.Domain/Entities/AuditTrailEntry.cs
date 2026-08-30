namespace SolarGridOps.Domain.Entities;

public class AuditTrailEntry : BaseEntity
{
    public string ActionKey { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
}