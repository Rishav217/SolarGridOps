using Microsoft.EntityFrameworkCore;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();
    public DbSet<InstallationSession> InstallationSessions => Set<InstallationSession>();
    public DbSet<InstallationEvidence> InstallationEvidence => Set<InstallationEvidence>();
    public DbSet<PanelAssignment> PanelAssignments => Set<PanelAssignment>();
    public DbSet<InverterAssignment> InverterAssignments => Set<InverterAssignment>();
    public DbSet<InvoiceRecord> InvoiceRecords => Set<InvoiceRecord>();
    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomer(modelBuilder);
        ConfigureCustomerDocument(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureProjectMilestone(modelBuilder);
        ConfigureInstallationSession(modelBuilder);
        ConfigureInstallationEvidence(modelBuilder);
        ConfigurePanelAssignment(modelBuilder);
        ConfigureInverterAssignment(modelBuilder);
        ConfigureInvoiceRecord(modelBuilder);
        ConfigurePaymentReceipt(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureRolePermission(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.AlternatePhone).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(250).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.State).HasMaxLength(100).IsRequired();
            e.Property(x => x.PanNumber).HasMaxLength(20);
            e.Property(x => x.BankAccountNumber).HasMaxLength(40);
            e.Property(x => x.BankName).HasMaxLength(120);
            e.Property(x => x.BankIFSC).HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.PhoneNumber);
        });
    }

    private static void ConfigureCustomerDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerDocument>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(250).IsRequired();
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.OriginalFileName).HasMaxLength(250);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => new { x.CustomerId, x.DocumentType, x.Version });

            e.HasOne(x => x.Customer)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProjectCode).HasMaxLength(40).IsRequired();
            e.Property(x => x.CapacityKW).HasPrecision(10, 2);
            e.Property(x => x.SiteAddress).HasMaxLength(250);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.ProjectCode).IsUnique();

            e.HasOne(x => x.Customer)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProjectMilestone(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectMilestone>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => new { x.ProjectId, x.Phase }).IsUnique();

            e.HasOne(x => x.Project)
                .WithMany(x => x.Milestones)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureInstallationSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstallationSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.WorkSummary).HasMaxLength(1000);

            e.HasOne(x => x.Project)
                .WithMany(x => x.InstallationSessions)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureInstallationEvidence(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstallationEvidence>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.FileName).HasMaxLength(250).IsRequired();
            e.Property(x => x.MediaType).HasMaxLength(30).IsRequired();
            e.Property(x => x.Latitude).HasPrecision(10, 7);
            e.Property(x => x.Longitude).HasPrecision(10, 7);
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.HasOne(x => x.InstallationSession)
                .WithMany(x => x.EvidenceItems)
                .HasForeignKey(x => x.InstallationSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePanelAssignment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PanelAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();
            e.Property(x => x.Brand).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.SerialNumber).IsUnique();

            e.HasOne(x => x.Project)
                .WithMany(x => x.Panels)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureInverterAssignment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InverterAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();
            e.Property(x => x.CapacityKva).HasPrecision(8, 2);
            e.Property(x => x.Brand).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.SerialNumber).IsUnique();

            e.HasOne(x => x.Project)
                .WithMany(x => x.Inverters)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureInvoiceRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvoiceRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InvoiceNumber).HasMaxLength(60).IsRequired();
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.FilePath).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => new { x.ProjectId, x.InvoiceNumber, x.Version }).IsUnique();

            e.HasOne(x => x.Project)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePaymentReceipt(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentReceipt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.PaymentMode).HasMaxLength(40).IsRequired();
            e.Property(x => x.ReceiptNumber).HasMaxLength(60);
            e.Property(x => x.FilePath).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => new { x.ProjectId, x.ReceiptDateUtc });

            e.HasOne(x => x.Project)
                .WithMany(x => x.PaymentReceipts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            e.Property(x => x.MobileNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.MobileNumber).IsUnique();
            e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RoleKey).HasMaxLength(50).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.RoleKey).IsUnique();
        });
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PermissionKey).HasMaxLength(100).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.PermissionKey).IsUnique();
        });
    }

    private static void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();

            e.HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();

            e.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            e.Property(x => x.RevokeReason).HasMaxLength(250);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

            e.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
