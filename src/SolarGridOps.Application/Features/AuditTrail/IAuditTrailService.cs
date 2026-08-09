namespace SolarGridOps.Application.Features.AuditTrail;

public interface IAuditTrailService
{
    Task RecordAsync(Guid? actorUserId, string actionKey, string entityType, Guid? entityId, string? details, CancellationToken cancellationToken = default);
}