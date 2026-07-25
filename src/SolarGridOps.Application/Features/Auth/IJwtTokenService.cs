using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Auth;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);
}
