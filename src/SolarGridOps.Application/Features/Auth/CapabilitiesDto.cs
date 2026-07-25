namespace SolarGridOps.Application.Features.Auth;

public class CapabilitiesDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
