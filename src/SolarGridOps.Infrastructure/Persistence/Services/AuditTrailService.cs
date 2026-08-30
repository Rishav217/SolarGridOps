using SolarGridOps.Application.Features.AuditTrail;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Services;

public class AuditTrailService : IAuditTrailService
{
    private readonly AppDbContext _dbContext;

    public AuditTrailService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(Guid? actorUserId, string actionKey, string entityType, Guid? entityId, string? details, CancellationToken cancellationToken = default)
    {
        var entry = new AuditTrailEntry
        {
            CreatedByUserId = actorUserId,
            ActionKey = actionKey.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.AuditTrailEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}