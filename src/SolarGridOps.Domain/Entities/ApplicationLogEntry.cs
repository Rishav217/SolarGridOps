namespace SolarGridOps.Domain.Entities;

public class ApplicationLogEntry : BaseEntity
{
    public string LogLevel { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ExceptionType { get; set; }
}