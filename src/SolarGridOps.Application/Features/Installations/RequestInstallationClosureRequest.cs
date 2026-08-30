namespace SolarGridOps.Application.Features.Installations;

public class RequestInstallationClosureRequest
{
    public string CustomerSignatureName { get; set; } = string.Empty;
    public string CustomerSignatureBase64 { get; set; } = string.Empty;
    public string? Notes { get; set; }
}