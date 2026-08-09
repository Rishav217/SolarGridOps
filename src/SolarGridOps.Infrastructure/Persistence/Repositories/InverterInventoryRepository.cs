using Microsoft.EntityFrameworkCore;
using SolarGridOps.Application.Features.Inventory;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Repositories;

public class InverterInventoryRepository : IInverterInventoryRepository
{
    private readonly AppDbContext _dbContext;

    public InverterInventoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.InverterAssignments.AnyAsync(x => x.SerialNumber == serialNumber && !x.IsDeleted, cancellationToken);
    }

    public async Task AddInverterAsync(InverterAssignment inverter, CancellationToken cancellationToken = default)
    {
        await _dbContext.InverterAssignments.AddAsync(inverter, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InverterAssignment>> ListInvertersByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InverterAssignments
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.AssignedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
