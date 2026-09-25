using Microsoft.EntityFrameworkCore;
using SolarGridOps.Application.Features.Dashboard;
using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Infrastructure.Persistence.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalProjects = await _dbContext.Projects.CountAsync(x => !x.IsDeleted, cancellationToken);
        var activeProjects = await _dbContext.Projects.CountAsync(x => !x.IsDeleted && x.CurrentPhase != ProjectPhase.Closed, cancellationToken);
        var installationSessions = await _dbContext.InstallationSessions.CountAsync(x => !x.IsDeleted, cancellationToken);
        var pendingClosureSessions = await _dbContext.InstallationSessions
            .CountAsync(x => !x.IsDeleted && x.ClosureStatus == InstallationClosureStatus.ClosedPendingApproval, cancellationToken);
        var totalPanels = await _dbContext.PanelAssignments.CountAsync(x => !x.IsDeleted, cancellationToken);
        var totalInverters = await _dbContext.InverterAssignments.CountAsync(x => !x.IsDeleted, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalProjectsCount = totalProjects,
            ActiveProjectsCount = activeProjects,
            InstallationSessionsCount = installationSessions,
            PendingClosureSessionsCount = pendingClosureSessions,
            TotalPanelsCount = totalPanels,
            TotalInvertersCount = totalInverters
        };
    }
}
