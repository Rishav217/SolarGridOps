using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Application.Features.Projects;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectDto>>>> List(CancellationToken cancellationToken)
    {
        var result = await _projectService.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProjectDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<ProjectDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<ProjectDto>.Ok(result.Value!));
    }

    [HttpGet("by-customer/{customerId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectDto>>>> ListByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _projectService.ListByCustomerAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<IReadOnlyList<ProjectDto>>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<IReadOnlyList<ProjectDto>>.Ok(result.Value ?? []));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error.Code == "CONFLICT")
            {
                return Conflict(ApiResponse<ProjectDto>.Fail(result.Error.Code, result.Error.Message));
            }

            if (result.Error.Code == "NOT_FOUND")
            {
                return NotFound(ApiResponse<ProjectDto>.Fail(result.Error.Code, result.Error.Message));
            }

            return BadRequest(ApiResponse<ProjectDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<ProjectDto>.Ok(result.Value));
    }
}
