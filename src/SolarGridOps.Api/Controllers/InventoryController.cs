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
    private readonly IInverterInventoryService _inverterInventoryService;

    public InventoryController(IInventoryService inventoryService, IInverterInventoryService inverterInventoryService)
    {
        _inventoryService = inventoryService;
        _inverterInventoryService = inverterInventoryService;
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

    [HttpGet("projects/{projectId:guid}/inverters")]
    [Authorize(Policy = PermissionPolicies.InventoryInvertersRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InverterInventoryDto>>>> ListInvertersByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _inverterInventoryService.ListInvertersByProjectAsync(projectId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<IReadOnlyList<InverterInventoryDto>>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<IReadOnlyList<InverterInventoryDto>>.Ok(result.Value ?? []));
    }

    [HttpPost("projects/{projectId:guid}/inverters")]
    [Authorize(Policy = PermissionPolicies.InventoryInvertersCreate)]
    public async Task<ActionResult<ApiResponse<InverterInventoryDto>>> AddInverter(Guid projectId, [FromBody] CreateInverterInventoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _inverterInventoryService.AddInverterAsync(projectId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error.Code == "CONFLICT")
            {
                return Conflict(ApiResponse<InverterInventoryDto>.Fail(result.Error.Code, result.Error.Message));
            }

            if (result.Error.Code == "NOT_FOUND")
            {
                return NotFound(ApiResponse<InverterInventoryDto>.Fail(result.Error.Code, result.Error.Message));
            }

            return BadRequest(ApiResponse<InverterInventoryDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return CreatedAtAction(nameof(ListInvertersByProject), new { projectId }, ApiResponse<InverterInventoryDto>.Ok(result.Value!));
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
