using Microsoft.EntityFrameworkCore;
using SolarGridOps.Application.Features.Installations;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Repositories;

public class InstallationRepository : IInstallationRepository
{
    private readonly AppDbContext _dbContext;

    public InstallationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken);
    }

    public Task<InstallationSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.InstallationSessions
            .Include(x => x.EvidenceItems)
            .FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<InstallationSession>> ListSessionsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InstallationSessions
            .AsNoTracking()
            .Include(x => x.EvidenceItems)
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.SessionDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddSessionAsync(InstallationSession session, CancellationToken cancellationToken = default)
    {
        await _dbContext.InstallationSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddEvidenceAsync(InstallationEvidence evidence, CancellationToken cancellationToken = default)
    {
        await _dbContext.InstallationEvidence.AddAsync(evidence, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InstallationEvidence>> ListEvidenceBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InstallationEvidence
            .AsNoTracking()
            .Where(x => x.InstallationSessionId == sessionId && !x.IsDeleted)
            .OrderByDescending(x => x.CapturedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
