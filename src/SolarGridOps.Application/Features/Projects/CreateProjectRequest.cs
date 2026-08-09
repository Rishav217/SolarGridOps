using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Application.Features.Projects;

public class CreateProjectRequest
{
    public Guid CustomerId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public decimal CapacityKW { get; set; }
    public ProjectPhase CurrentPhase { get; set; } = ProjectPhase.Quotation;
    public DateTime? InstallationStartDate { get; set; }
    public DateTime? InstallationEndDate { get; set; }
    public string? SiteAddress { get; set; }
    public string? Notes { get; set; }
}
