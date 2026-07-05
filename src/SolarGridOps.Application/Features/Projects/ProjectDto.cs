using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Application.Features.Projects;

public class ProjectDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public decimal CapacityKW { get; set; }
    public ProjectPhase CurrentPhase { get; set; }
    public DateTime? InstallationStartDate { get; set; }
    public DateTime? InstallationEndDate { get; set; }
    public string? SiteAddress { get; set; }
    public string? Notes { get; set; }
}
