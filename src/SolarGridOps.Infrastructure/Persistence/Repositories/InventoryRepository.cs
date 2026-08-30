using Microsoft.EntityFrameworkCore;
using SolarGridOps.Application.Features.Inventory;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.PanelAssignments.AnyAsync(x => x.SerialNumber == serialNumber && !x.IsDeleted, cancellationToken);
    }

    public async Task AddPanelAsync(PanelAssignment panel, CancellationToken cancellationToken = default)
    {
        await _dbContext.PanelAssignments.AddAsync(panel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PanelAssignment>> ListPanelsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PanelAssignments
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.AssignedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<PanelAssignment?> GetPanelByIdAsync(Guid panelId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PanelAssignments
            .FirstOrDefaultAsync(x => x.Id == panelId && !x.IsDeleted, cancellationToken);
    }

    public Task<InverterAssignment?> GetInverterByIdAsync(Guid inverterId, CancellationToken cancellationToken = default)
    {
        return _dbContext.InverterAssignments
            .FirstOrDefaultAsync(x => x.Id == inverterId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> SerialNumberExistsForOtherPanelAsync(Guid panelId, string serialNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.PanelAssignments
            .AnyAsync(x => x.Id != panelId && x.SerialNumber == serialNumber && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> PanelExistsInProjectAsync(Guid panelId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PanelAssignments
            .AnyAsync(x => x.Id == panelId && x.ProjectId == projectId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> InverterExistsInProjectAsync(Guid inverterId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.InverterAssignments
            .AnyAsync(x => x.Id == inverterId && x.ProjectId == projectId && !x.IsDeleted, cancellationToken);
    }

    public async Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryMovements.AddAsync(movement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryMovement>> ListMovementsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryMovements
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.MovedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryMovement>> ListMovementsByItemAsync(string itemType, Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryMovements
            .AsNoTracking()
            .Where(x => x.ItemType == itemType && x.ItemId == itemId && !x.IsDeleted)
            .OrderByDescending(x => x.MovedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
