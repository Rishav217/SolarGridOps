using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Domain.Entities;

public class Project : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public decimal CapacityKW { get; set; }
    public ProjectPhase CurrentPhase { get; set; } = ProjectPhase.Quotation;
    public DateTime? InstallationStartDate { get; set; }
    public DateTime? InstallationEndDate { get; set; }
    public string? SiteAddress { get; set; }
    public string? Notes { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    public ICollection<InstallationSession> InstallationSessions { get; set; } = new List<InstallationSession>();
    public ICollection<PanelAssignment> Panels { get; set; } = new List<PanelAssignment>();
    public ICollection<InverterAssignment> Inverters { get; set; } = new List<InverterAssignment>();
    public ICollection<InvoiceRecord> Invoices { get; set; } = new List<InvoiceRecord>();
    public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
}