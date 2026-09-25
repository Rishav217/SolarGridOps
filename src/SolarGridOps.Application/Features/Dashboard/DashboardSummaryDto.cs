namespace SolarGridOps.Application.Features.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProjectsCount { get; set; }
    public int ActiveProjectsCount { get; set; }
    public int InstallationSessionsCount { get; set; }
    public int PendingClosureSessionsCount { get; set; }
    public int TotalPanelsCount { get; set; }
    public int TotalInvertersCount { get; set; }
}
