using Microsoft.EntityFrameworkCore;
using SolarGridOps.Application.Features.Auth;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _dbContext;

    public AuthRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdentifierWithSecurityAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return SecurityUserQuery()
            .FirstOrDefaultAsync(x =>
                x.IsActive &&
                (x.MobileNumber == identifier || (x.Email != null && x.Email == identifier)),
                cancellationToken);
    }

    public Task<User?> GetByIdWithSecurityAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return SecurityUserQuery().FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);
    }

    public Task<RefreshToken?> GetValidRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                        .ThenInclude(x => x.RolePermissions)
                            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == refreshTokenHash &&
                x.RevokedAtUtc == null &&
                x.ExpiresAtUtc > DateTime.UtcNow &&
                x.User.IsActive,
                cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<User> SecurityUserQuery()
    {
        return _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission);
    }
}
