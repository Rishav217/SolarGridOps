using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Api.Security;
using SolarGridOps.Application.Features.AuditTrail;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/admin/audit-trail")]
[Authorize]
public class AdminAuditController : ControllerBase
{
    private readonly IAuditTrailService _auditTrailService;

    public AdminAuditController(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.AuditTrailRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AuditTrailEntryDto>>>> ListRecent([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var entries = await _auditTrailService.ListRecentAsync(take, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AuditTrailEntryDto>>.Ok(entries));
    }
}
