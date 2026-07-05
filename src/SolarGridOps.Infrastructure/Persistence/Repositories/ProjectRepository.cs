using Microsoft.EntityFrameworkCore;
using SolarGridOps.Application.Features.Projects;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.AnyAsync(x => x.Id == customerId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> ProjectCodeExistsAsync(string projectCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AnyAsync(x => x.ProjectCode == projectCode && !x.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
