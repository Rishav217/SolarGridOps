namespace SolarGridOps.Application.Features.Auth;

public class LoginRequest
{
    public string UsernameOrMobile { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
