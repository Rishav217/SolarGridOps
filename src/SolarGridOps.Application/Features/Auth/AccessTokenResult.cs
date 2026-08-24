namespace SolarGridOps.Application.Features.Auth;

public class AccessTokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
