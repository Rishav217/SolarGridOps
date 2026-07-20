using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Application.Features.Projects;

public class UpdateProjectPhaseRequest
{
    public ProjectPhase CurrentPhase { get; set; }
    public DateTime? InstallationStartDate { get; set; }
    public DateTime? InstallationEndDate { get; set; }
    public string? Notes { get; set; }
}
