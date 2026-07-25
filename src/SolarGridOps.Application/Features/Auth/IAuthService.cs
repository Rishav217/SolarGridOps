using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Auth;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result<CapabilitiesDto>> GetCapabilitiesAsync(Guid userId, CancellationToken cancellationToken = default);
}
