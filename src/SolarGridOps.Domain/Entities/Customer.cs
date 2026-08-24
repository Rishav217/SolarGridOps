namespace SolarGridOps.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? PanNumber { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankIFSC { get; set; }
    public string? Notes { get; set; }

    public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}