using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Installations;

public interface IInstallationRepository
{
    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<InstallationSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationSession>> ListSessionsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddSessionAsync(InstallationSession session, CancellationToken cancellationToken = default);
    Task AddEvidenceAsync(InstallationEvidence evidence, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationEvidence>> ListEvidenceBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
