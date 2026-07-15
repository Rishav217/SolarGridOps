using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Api.Security;
using SolarGridOps.Application.Features.AuditTrail;
using SolarGridOps.Application.Features.Installations;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/installations")]
[Authorize]
public class InstallationsController : ControllerBase
{
    private readonly IInstallationService _installationService;
    private readonly IAuditTrailService _auditTrailService;

    public InstallationsController(IInstallationService installationService, IAuditTrailService auditTrailService)
    {
        _installationService = installationService;
        _auditTrailService = auditTrailService;
    }

    [HttpGet("projects/{projectId:guid}/sessions")]
    [Authorize(Policy = PermissionPolicies.InstallationsSessionsRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InstallationSessionDto>>>> ListSessionsByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _installationService.ListSessionsByProjectAsync(projectId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<IReadOnlyList<InstallationSessionDto>>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<IReadOnlyList<InstallationSessionDto>>.Ok(result.Value ?? []));
    }

    [HttpPost("sessions")]
    [Authorize(Policy = PermissionPolicies.InstallationsSessionsCreate)]
    public async Task<ActionResult<ApiResponse<InstallationSessionDto>>> CreateSession([FromBody] CreateInstallationSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _installationService.CreateSessionAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<InstallationSessionDto>.Fail(result.Error.Code, result.Error.Message));
        }

        await RecordAuditAsync("installations.session.created", "installation_session", result.Value!.Id, $"projectId={result.Value.ProjectId};sessionDateUtc={result.Value.SessionDateUtc:O}", cancellationToken);

        return CreatedAtAction(
            nameof(ListSessionsByProject),
            new { projectId = result.Value!.ProjectId },
            ApiResponse<InstallationSessionDto>.Ok(result.Value));
    }

    [HttpGet("sessions/{sessionId:guid}/evidence")]
    [Authorize(Policy = PermissionPolicies.InstallationsEvidenceRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InstallationEvidenceDto>>>> ListEvidenceBySession(Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _installationService.ListEvidenceBySessionAsync(sessionId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<IReadOnlyList<InstallationEvidenceDto>>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<IReadOnlyList<InstallationEvidenceDto>>.Ok(result.Value ?? []));
    }

    [HttpPost("sessions/{sessionId:guid}/evidence")]
    [Authorize(Policy = PermissionPolicies.InstallationsEvidenceCreate)]
    public async Task<ActionResult<ApiResponse<InstallationEvidenceDto>>> AddEvidence(Guid sessionId, [FromBody] AddInstallationEvidenceRequest request, CancellationToken cancellationToken)
    {
        var result = await _installationService.AddEvidenceAsync(sessionId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<InstallationEvidenceDto>.Fail(result.Error.Code, result.Error.Message));
        }

        await RecordAuditAsync("installations.evidence.created", "installation_evidence", result.Value!.Id, $"sessionId={sessionId};mediaType={result.Value.MediaType}", cancellationToken);

        return CreatedAtAction(
            nameof(ListEvidenceBySession),
            new { sessionId },
            ApiResponse<InstallationEvidenceDto>.Ok(result.Value!));
    }

    [HttpPatch("sessions/{sessionId:guid}")]
    [Authorize(Policy = PermissionPolicies.InstallationsSessionsUpdate)]
    public async Task<ActionResult<ApiResponse<InstallationSessionDto>>> UpdateSession(Guid sessionId, [FromBody] UpdateInstallationSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _installationService.UpdateSessionAsync(sessionId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error.Code == "NOT_FOUND")
            {
                return NotFound(ApiResponse<InstallationSessionDto>.Fail(result.Error.Code, result.Error.Message));
            }

            return BadRequest(ApiResponse<InstallationSessionDto>.Fail(result.Error.Code, result.Error.Message));
        }

        await RecordAuditAsync("installations.session.updated", "installation_session", result.Value!.Id, $"sessionDateUtc={result.Value.SessionDateUtc:O};completed={result.Value.IsCompletedForDay}", cancellationToken);
        return Ok(ApiResponse<InstallationSessionDto>.Ok(result.Value!));
    }

    private async Task RecordAuditAsync(string actionKey, string entityType, Guid entityId, string? details, CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        await _auditTrailService.RecordAsync(actorUserId, actionKey, entityType, entityId, details, cancellationToken);
    }

    private Guid? GetActorUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
