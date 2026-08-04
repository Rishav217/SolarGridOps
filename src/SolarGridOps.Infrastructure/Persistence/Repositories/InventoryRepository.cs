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
}
