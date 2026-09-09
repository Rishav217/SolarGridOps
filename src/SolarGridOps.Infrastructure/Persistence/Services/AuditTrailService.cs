using SolarGridOps.Application.Features.AuditTrail;
using SolarGridOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<AuditTrailEntryDto>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var clampedTake = Math.Clamp(take, 1, 250);

        var query =
            from audit in _dbContext.AuditTrailEntries.AsNoTracking()
            join user in _dbContext.Users.AsNoTracking() on audit.CreatedByUserId equals user.Id into users
            from actor in users.DefaultIfEmpty()
            orderby audit.CreatedAtUtc descending
            select new AuditTrailEntryDto
            {
                Id = audit.Id,
                CreatedAtUtc = audit.CreatedAtUtc,
                ActionKey = audit.ActionKey,
                EntityType = audit.EntityType,
                EntityId = audit.EntityId,
                Details = audit.Details,
                ActorUserId = audit.CreatedByUserId,
                ActorName = actor != null && !string.IsNullOrWhiteSpace(actor.FullName)
                    ? actor.FullName
                    : "System"
            };

        return await query.Take(clampedTake).ToListAsync(cancellationToken);
    }
}