using SolarGridOps.Application.Features.Logging;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence.Services;

public class AppLogService : IAppLogService
{
    private readonly AppDbContext _dbContext;

    public AppLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(string logLevel, string eventKey, string category, string message, string? details = null, string? exceptionType = null, Guid? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var entry = new ApplicationLogEntry
        {
            CreatedByUserId = actorUserId,
            LogLevel = logLevel.Trim(),
            EventKey = eventKey.Trim(),
            Category = category.Trim(),
            Message = message.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            ExceptionType = string.IsNullOrWhiteSpace(exceptionType) ? null : exceptionType.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.ApplicationLogEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}