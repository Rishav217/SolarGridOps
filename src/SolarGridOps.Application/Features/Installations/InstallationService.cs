using SolarGridOps.Application.Common;
using SolarGridOps.Application.Features.Logging;
using SolarGridOps.Domain.Entities;
using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Application.Features.Installations;

public class InstallationService : IInstallationService
{
    private readonly IInstallationRepository _installationRepository;
    private readonly IAppLogService _appLogService;

    public InstallationService(IInstallationRepository installationRepository, IAppLogService appLogService)
    {
        _installationRepository = installationRepository;
        _appLogService = appLogService;
    }

    public async Task<Result<InstallationSessionDto>> CreateSessionAsync(CreateInstallationSessionRequest request, CancellationToken cancellationToken = default)
    {
        var projectExists = await _installationRepository.ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            await _appLogService.WriteAsync("Warning", "installations.session.project_missing", "installations", "Project not found while creating installation session.", $"projectId={request.ProjectId}", cancellationToken: cancellationToken);
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
        await _appLogService.WriteAsync("Information", "installations.session.created", "installations", "Installation session created.", $"projectId={session.ProjectId};sessionId={session.Id}", cancellationToken: cancellationToken);
        return Result<InstallationSessionDto>.Success(Map(session));
    }

    public async Task<Result<IReadOnlyList<InstallationSessionDto>>> ListSessionsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _installationRepository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            await _appLogService.WriteAsync("Warning", "installations.session.project_missing", "installations", "Project not found while listing installation sessions.", $"projectId={projectId}", cancellationToken: cancellationToken);
            return Result<IReadOnlyList<InstallationSessionDto>>.Failure(Error.NotFound("Project not found."));
        }

        var sessions = await _installationRepository.ListSessionsByProjectAsync(projectId, cancellationToken);
        var technicianIds = sessions.Where(x => x.TechnicianUserId.HasValue).Select(x => x.TechnicianUserId!.Value);
        var technicianNames = await _installationRepository.GetUserNamesAsync(technicianIds, cancellationToken);

        var dtos = sessions.Select(session =>
        {
            var dto = Map(session);
            if (session.TechnicianUserId.HasValue && technicianNames.TryGetValue(session.TechnicianUserId.Value, out var name))
            {
                dto.TechnicianName = name;
            }

            return dto;
        }).ToList();

        return Result<IReadOnlyList<InstallationSessionDto>>.Success(dtos);
    }

    public async Task<Result<InstallationEvidenceDto>> AddEvidenceAsync(Guid sessionId, AddInstallationEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            await _appLogService.WriteAsync("Warning", "installations.evidence.session_missing", "installations", "Installation session not found while adding evidence.", $"sessionId={sessionId}", cancellationToken: cancellationToken);
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
        await _appLogService.WriteAsync("Information", "installations.evidence.created", "installations", "Installation evidence added.", $"sessionId={sessionId};mediaType={evidence.MediaType};fileName={evidence.FileName}", cancellationToken: cancellationToken);
        return Result<InstallationEvidenceDto>.Success(Map(evidence));
    }

    public async Task<Result<IReadOnlyList<InstallationEvidenceDto>>> ListEvidenceBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            await _appLogService.WriteAsync("Warning", "installations.evidence.session_missing", "installations", "Installation session not found while listing evidence.", $"sessionId={sessionId}", cancellationToken: cancellationToken);
            return Result<IReadOnlyList<InstallationEvidenceDto>>.Failure(Error.NotFound("Installation session not found."));
        }

        var evidenceItems = await _installationRepository.ListEvidenceBySessionAsync(sessionId, cancellationToken);
        return Result<IReadOnlyList<InstallationEvidenceDto>>.Success(evidenceItems.Select(Map).ToList());
    }

    public async Task<Result<InstallationSessionDto>> UpdateSessionAsync(Guid sessionId, UpdateInstallationSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            await _appLogService.WriteAsync("Warning", "installations.session.session_missing", "installations", "Installation session not found while updating session.", $"sessionId={sessionId}", cancellationToken: cancellationToken);
            return Result<InstallationSessionDto>.Failure(Error.NotFound("Installation session not found."));
        }

        if (request.TechnicianUserId.HasValue)
        {
            session.TechnicianUserId = request.TechnicianUserId;
        }

        if (request.SessionDateUtc.HasValue)
        {
            session.SessionDateUtc = request.SessionDateUtc.Value;
        }

        if (request.WorkSummary is not null)
        {
            session.WorkSummary = request.WorkSummary.Trim();
        }

        if (request.IsCompletedForDay.HasValue)
        {
            session.IsCompletedForDay = request.IsCompletedForDay.Value;
        }

        await _installationRepository.SaveChangesAsync(cancellationToken);
        await _appLogService.WriteAsync("Information", "installations.session.updated", "installations", "Installation session updated.", $"sessionId={session.Id};completed={session.IsCompletedForDay}", cancellationToken: cancellationToken);
        return Result<InstallationSessionDto>.Success(Map(session));
    }

    public async Task<Result<InstallationSessionDto>> RequestClosureAsync(Guid sessionId, RequestInstallationClosureRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            await _appLogService.WriteAsync("Warning", "installations.closure.session_missing", "installations", "Installation session not found while requesting closure.", $"sessionId={sessionId}", cancellationToken: cancellationToken);
            return Result<InstallationSessionDto>.Failure(Error.NotFound("Installation session not found."));
        }

        if (session.ClosureStatus == InstallationClosureStatus.ClosedApproved)
        {
            return Result<InstallationSessionDto>.Failure(Error.Conflict("Installation session is already approved and closed."));
        }

        session.ClosureStatus = InstallationClosureStatus.ClosedPendingApproval;
        session.ClosureRequestedAtUtc = DateTime.UtcNow;
        session.ClosureRequestedByUserId = actorUserId;
        session.CustomerSignatureName = request.CustomerSignatureName.Trim();
        session.CustomerSignatureBase64 = request.CustomerSignatureBase64.Trim();
        session.ClosureNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _installationRepository.SaveChangesAsync(cancellationToken);
        await _appLogService.WriteAsync("Information", "installations.closure.requested", "installations", "Installation closure requested.", $"sessionId={session.Id};actorUserId={actorUserId}", cancellationToken: cancellationToken);

        return Result<InstallationSessionDto>.Success(Map(session));
    }

    public async Task<Result<InstallationSessionDto>> ApproveClosureAsync(Guid sessionId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var session = await _installationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            await _appLogService.WriteAsync("Warning", "installations.closure.session_missing", "installations", "Installation session not found while approving closure.", $"sessionId={sessionId}", cancellationToken: cancellationToken);
            return Result<InstallationSessionDto>.Failure(Error.NotFound("Installation session not found."));
        }

        if (session.ClosureStatus != InstallationClosureStatus.ClosedPendingApproval)
        {
            return Result<InstallationSessionDto>.Failure(Error.Validation("Installation session is not pending approval."));
        }

        session.ClosureStatus = InstallationClosureStatus.ClosedApproved;
        session.ClosureApprovedAtUtc = DateTime.UtcNow;
        session.ClosureApprovedByUserId = actorUserId;

        await _installationRepository.SaveChangesAsync(cancellationToken);
        await _appLogService.WriteAsync("Information", "installations.closure.approved", "installations", "Installation closure approved.", $"sessionId={session.Id};actorUserId={actorUserId}", cancellationToken: cancellationToken);

        return Result<InstallationSessionDto>.Success(Map(session));
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
            ClosureStatus = session.ClosureStatus,
            ClosureRequestedAtUtc = session.ClosureRequestedAtUtc,
            ClosureApprovedAtUtc = session.ClosureApprovedAtUtc,
            CustomerSignatureName = session.CustomerSignatureName,
            ClosureNotes = session.ClosureNotes,
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
