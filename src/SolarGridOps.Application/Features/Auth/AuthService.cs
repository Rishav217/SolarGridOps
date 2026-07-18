using System.Security.Cryptography;
using System.Text;
using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IAuthRepository authRepository, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher)
    {
        _authRepository = authRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var identifier = request.UsernameOrMobile.Trim();
        var user = await _authRepository.GetByIdentifierWithSecurityAsync(identifier, cancellationToken);
        if (user is null)
        {
            return Result<AuthResponseDto>.Failure(Error.Unauthorized("Invalid credentials."));
        }

        var validPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
        {
            return Result<AuthResponseDto>.Failure(Error.Unauthorized("Invalid credentials."));
        }

        var roles = user.UserRoles.Select(x => x.Role.RoleKey).Distinct().OrderBy(x => x).ToList();
        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.PermissionKey)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var token = _jwtTokenService.CreateAccessToken(user, roles, permissions);
        var refreshToken = CreateRefreshToken();

        await _authRepository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashRefreshToken(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(15)
        }, cancellationToken);

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            AccessToken = token.Token,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = token.ExpiresAtUtc,
            Roles = roles,
            Permissions = permissions
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var providedToken = request.RefreshToken.Trim();
        var providedHash = HashRefreshToken(providedToken);

        var existing = await _authRepository.GetValidRefreshTokenAsync(providedHash, cancellationToken);
        if (existing is null)
        {
            return Result<AuthResponseDto>.Failure(Error.Unauthorized("Invalid or expired refresh token."));
        }

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.RevokeReason = "Rotated";

        var user = existing.User;
        var roles = user.UserRoles.Select(x => x.Role.RoleKey).Distinct().OrderBy(x => x).ToList();
        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.PermissionKey)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var token = _jwtTokenService.CreateAccessToken(user, roles, permissions);
        var nextRefreshToken = CreateRefreshToken();

        await _authRepository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashRefreshToken(nextRefreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(15)
        }, cancellationToken);

        await _authRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            AccessToken = token.Token,
            RefreshToken = nextRefreshToken,
            AccessTokenExpiresAtUtc = token.ExpiresAtUtc,
            Roles = roles,
            Permissions = permissions
        });
    }

    public async Task<Result> LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var providedToken = request.RefreshToken.Trim();
        var providedHash = HashRefreshToken(providedToken);

        var existing = await _authRepository.GetRefreshTokenAsync(providedHash, cancellationToken);
        if (existing is null)
        {
            // Idempotent logout to avoid token enumeration.
            return Result.Success();
        }

        if (existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.RevokeReason = "Logout";
            await _authRepository.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<CapabilitiesDto>> GetCapabilitiesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetByIdWithSecurityAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<CapabilitiesDto>.Failure(Error.NotFound("User not found."));
        }

        var roles = user.UserRoles.Select(x => x.Role.RoleKey).Distinct().OrderBy(x => x).ToList();
        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.PermissionKey)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return Result<CapabilitiesDto>.Success(new CapabilitiesDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Roles = roles,
            Permissions = permissions
        });
    }

    private static string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}
