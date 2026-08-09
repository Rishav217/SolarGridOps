using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGridOps.Api.Models;
using SolarGridOps.Api.Security;
using SolarGridOps.Application.Features.Auth;

namespace SolarGridOps.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!));
    }

    [Authorize]
    [Authorize(Policy = PermissionPolicies.AuthCapabilities)]
    [HttpGet("capabilities")]
    public async Task<ActionResult<ApiResponse<CapabilitiesDto>>> Capabilities(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<CapabilitiesDto>.Fail("UNAUTHORIZED", "Invalid token subject."));
        }

        var result = await _authService.GetCapabilitiesAsync(userId, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<CapabilitiesDto>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<CapabilitiesDto>.Ok(result.Value!));
    }
}
