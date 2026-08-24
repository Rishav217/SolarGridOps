using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Auth;

public interface IAuthRepository
{
    Task<User?> GetByIdentifierWithSecurityAsync(string identifier, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithSecurityAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
