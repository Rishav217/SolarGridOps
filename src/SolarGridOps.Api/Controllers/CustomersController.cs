using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Api.Security;
using SolarGridOps.Application.Features.Customers;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.CustomersRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CustomerDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _customerService.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CustomerDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.CustomersRead)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<CustomerDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.CustomersCreate)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error.Code == "CONFLICT")
            {
                return Conflict(ApiResponse<CustomerDto>.Fail(result.Error.Code, result.Error.Message));
            }

            return BadRequest(ApiResponse<CustomerDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<CustomerDto>.Ok(result.Value));
    }
}
