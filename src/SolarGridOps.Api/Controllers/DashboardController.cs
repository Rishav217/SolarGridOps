using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Application.Features.Dashboard;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }
}
