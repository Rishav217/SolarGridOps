using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Api.Security;
using SolarGridOps.Application.Features.Inventory;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("projects/{projectId:guid}/panels")]
    [Authorize(Policy = PermissionPolicies.InventoryPanelsRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PanelInventoryDto>>>> ListPanelsByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.ListPanelsByProjectAsync(projectId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<IReadOnlyList<PanelInventoryDto>>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<IReadOnlyList<PanelInventoryDto>>.Ok(result.Value ?? []));
    }

    [HttpPost("projects/{projectId:guid}/panels")]
    [Authorize(Policy = PermissionPolicies.InventoryPanelsCreate)]
    public async Task<ActionResult<ApiResponse<PanelInventoryDto>>> AddPanel(Guid projectId, [FromBody] CreatePanelInventoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.AddPanelAsync(projectId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error.Code == "CONFLICT")
            {
                return Conflict(ApiResponse<PanelInventoryDto>.Fail(result.Error.Code, result.Error.Message));
            }

            if (result.Error.Code == "NOT_FOUND")
            {
                return NotFound(ApiResponse<PanelInventoryDto>.Fail(result.Error.Code, result.Error.Message));
            }

            return BadRequest(ApiResponse<PanelInventoryDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return CreatedAtAction(nameof(ListPanelsByProject), new { projectId }, ApiResponse<PanelInventoryDto>.Ok(result.Value!));
    }

    [HttpPatch("panels/{panelId:guid}")]
    [Authorize(Policy = PermissionPolicies.InventoryPanelsUpdate)]
    public async Task<ActionResult<ApiResponse<PanelInventoryDto>>> UpdatePanel(Guid panelId, [FromBody] UpdatePanelInventoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.UpdatePanelAsync(panelId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error.Code == "CONFLICT")
            {
                return Conflict(ApiResponse<PanelInventoryDto>.Fail(result.Error.Code, result.Error.Message));
            }

            if (result.Error.Code == "NOT_FOUND")
            {
                return NotFound(ApiResponse<PanelInventoryDto>.Fail(result.Error.Code, result.Error.Message));
            }

            return BadRequest(ApiResponse<PanelInventoryDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<PanelInventoryDto>.Ok(result.Value!));
    }

    [HttpDelete("panels/{panelId:guid}")]
    [Authorize(Policy = PermissionPolicies.InventoryPanelsDelete)]
    public async Task<IActionResult> RemovePanel(Guid panelId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.RemovePanelAsync(panelId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<string>.Fail(result.Error.Code, result.Error.Message));
        }

        return NoContent();
    }
}
