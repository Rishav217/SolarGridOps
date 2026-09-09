namespace SolarGridOps.Application.Features.AuditTrail;

public interface IAuditTrailService
{
    Task RecordAsync(Guid? actorUserId, string actionKey, string entityType, Guid? entityId, string? details, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditTrailEntryDto>> ListRecentAsync(int take, CancellationToken cancellationToken = default);
}