using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Installations;

public interface IInstallationService
{
    Task<Result<InstallationSessionDto>> CreateSessionAsync(CreateInstallationSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InstallationSessionDto>>> ListSessionsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<InstallationEvidenceDto>> AddEvidenceAsync(Guid sessionId, AddInstallationEvidenceRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InstallationEvidenceDto>>> ListEvidenceBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<InstallationSessionDto>> UpdateSessionAsync(Guid sessionId, UpdateInstallationSessionRequest request, CancellationToken cancellationToken = default);
}
