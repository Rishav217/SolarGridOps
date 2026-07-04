using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Installations;

public class InstallationService : IInstallationService
{
    private readonly IInstallationRepository _installationRepository;

    public InstallationService(IInstallationRepository installationRepository)
    {
        _installationRepository = installationRepository;
    }

    public async Task<Result<InstallationSessionDto>> CreateSessionAsync(CreateInstallationSessionRequest request, CancellationToken cancellationToken = default)
    {
        var projectExists = await _installationRepository.ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return Result<InstallationSessionDto>.Failure(Error.NotFound("Project not found."));
        }

        var session = new InstallationSession
        {
            ProjectId = request.ProjectId,
            TechnicianUserId = request.TechnicianUserId,
            SessionDateUtc = request.SessionDateUtc ?? DateTime.UtcNow,
            WorkSummary = request.WorkSummary?.Trim(),
            IsCompletedForDay = request.IsCompletedForDay
        };

        await _installationRepository.AddSessionAsync(session, cancellationToken);
        return Result<InstallationSessionDto>.Success(Map(session));
    }

    public async Task<Result<IReadOnlyList<InstallationSessionDto>>> ListSessionsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _installationRepository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result<IReadOnlyList<InstallationSessionDto>>.Failure(Error.NotFound("Project not found."));
        }

        var sessions = await _installationRepository.ListSessionsByProjectAsync(projectId, cancellationToken);
        return Result<IReadOnlyList<InstallationSessionDto>>.Success(sessions.Select(Map).ToList());
    }

    public async Task<Result<InstallationEvidenceDto>> AddEvidenceAsync(Guid sessionId, AddInstallationEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return Result<InstallationEvidenceDto>.Failure(Error.NotFound("Installation session not found."));
        }

        var evidence = new InstallationEvidence
        {
            InstallationSessionId = sessionId,
            FilePath = request.FilePath.Trim(),
            FileName = request.FileName.Trim(),
            MediaType = request.MediaType.Trim(),
            CapturedAtUtc = request.CapturedAtUtc ?? DateTime.UtcNow,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Notes = request.Notes?.Trim()
        };

        await _installationRepository.AddEvidenceAsync(evidence, cancellationToken);
        return Result<InstallationEvidenceDto>.Success(Map(evidence));
    }

    public async Task<Result<IReadOnlyList<InstallationEvidenceDto>>> ListEvidenceBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return Result<IReadOnlyList<InstallationEvidenceDto>>.Failure(Error.NotFound("Installation session not found."));
        }

        var evidenceItems = await _installationRepository.ListEvidenceBySessionAsync(sessionId, cancellationToken);
        return Result<IReadOnlyList<InstallationEvidenceDto>>.Success(evidenceItems.Select(Map).ToList());
    }

    private static InstallationSessionDto Map(InstallationSession session)
    {
        return new InstallationSessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            TechnicianUserId = session.TechnicianUserId,
            SessionDateUtc = session.SessionDateUtc,
            WorkSummary = session.WorkSummary,
            IsCompletedForDay = session.IsCompletedForDay,
            EvidenceCount = session.EvidenceItems.Count
        };
    }

    private static InstallationEvidenceDto Map(InstallationEvidence evidence)
    {
        return new InstallationEvidenceDto
        {
            Id = evidence.Id,
            InstallationSessionId = evidence.InstallationSessionId,
            FilePath = evidence.FilePath,
            FileName = evidence.FileName,
            MediaType = evidence.MediaType,
            CapturedAtUtc = evidence.CapturedAtUtc,
            Latitude = evidence.Latitude,
            Longitude = evidence.Longitude,
            Notes = evidence.Notes
        };
    }
}
