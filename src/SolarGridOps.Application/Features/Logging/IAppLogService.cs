namespace SolarGridOps.Application.Features.Logging;

public interface IAppLogService
{
    Task WriteAsync(string logLevel, string eventKey, string category, string message, string? details = null, string? exceptionType = null, Guid? actorUserId = null, CancellationToken cancellationToken = default);
}