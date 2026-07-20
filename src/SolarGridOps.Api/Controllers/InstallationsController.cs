using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Application.Features.Installations;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/installations")]
[Authorize]
public class InstallationsController : ControllerBase
{
    private readonly IInstallationService _installationService;

    public InstallationsController(IInstallationService installationService)
    {
        _installationService = installationService;
    }

    [HttpGet("projects/{projectId:guid}/sessions")]
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
    public async Task<ActionResult<ApiResponse<InstallationSessionDto>>> CreateSession([FromBody] CreateInstallationSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _installationService.CreateSessionAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<InstallationSessionDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return CreatedAtAction(
            nameof(ListSessionsByProject),
            new { projectId = result.Value!.ProjectId },
            ApiResponse<InstallationSessionDto>.Ok(result.Value));
    }

    [HttpGet("sessions/{sessionId:guid}/evidence")]
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
    public async Task<ActionResult<ApiResponse<InstallationEvidenceDto>>> AddEvidence(Guid sessionId, [FromBody] AddInstallationEvidenceRequest request, CancellationToken cancellationToken)
    {
        var result = await _installationService.AddEvidenceAsync(sessionId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<InstallationEvidenceDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return CreatedAtAction(
            nameof(ListEvidenceBySession),
            new { sessionId },
            ApiResponse<InstallationEvidenceDto>.Ok(result.Value!));
    }

    [HttpPatch("sessions/{sessionId:guid}")]
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

        return Ok(ApiResponse<InstallationSessionDto>.Ok(result.Value!));
    }
}
