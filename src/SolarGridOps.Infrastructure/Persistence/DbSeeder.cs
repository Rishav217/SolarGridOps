using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Domain.Entities;
using SolarGridOps.Domain.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SolarGridOps.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        var requiredPermissions = new (string Key, string Name)[]
        {
            ("customers.read", "Read Customers"),
            ("customers.create", "Create Customers"),
            ("projects.read", "Read Projects"),
            ("projects.create", "Create Projects"),
            ("projects.updatePhase", "Update Project Phase"),
            ("installations.sessions.read", "Read Installation Sessions"),
            ("installations.sessions.create", "Create Installation Sessions"),
            ("installations.sessions.update", "Update Installation Sessions"),
            ("installations.evidence.read", "Read Installation Evidence"),
            ("installations.evidence.create", "Create Installation Evidence"),
            ("installations.closure.request", "Request Installation Closure"),
            ("installations.closure.approve", "Approve Installation Closure"),
            ("inventory.panels.read", "Read Inventory Panels"),
            ("inventory.panels.create", "Create Inventory Panels"),
            ("inventory.panels.update", "Update Inventory Panels"),
            ("inventory.panels.delete", "Delete Inventory Panels"),
            ("inventory.inverters.read", "Read Inventory Inverters"),
            ("inventory.inverters.create", "Create Inventory Inverters"),
            ("inventory.movements.read", "Read Inventory Movements"),
            ("inventory.movements.create", "Create Inventory Movements"),
            ("audit.trail.read", "Read Audit Trail"),
            ("auth.capabilities", "Read Capabilities")
        };

        var existingPermissions = await db.Permissions
            .ToDictionaryAsync(x => x.PermissionKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var permission in requiredPermissions)
        {
            if (!existingPermissions.ContainsKey(permission.Key))
            {
                var entity = new Permission
                {
                    PermissionKey = permission.Key,
                    DisplayName = permission.Name
                };
                db.Permissions.Add(entity);
                existingPermissions[permission.Key] = entity;
            }
        }

        var role = await db.Roles.FirstOrDefaultAsync(x => x.RoleKey == "owner_admin", cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                RoleKey = "owner_admin",
                DisplayName = "Owner Admin",
                RoleType = RoleType.OwnerAdmin
            };
            db.Roles.Add(role);
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.MobileNumber == "9999999999", cancellationToken)
            ?? await db.Users.FirstOrDefaultAsync(x => x.Email == "owner@solargridops.local", cancellationToken);

        if (user is null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            user = new User
            {
                FullName = "Owner Admin",
                MobileNumber = "9999999999",
                Email = "owner@solargridops.local",
                PasswordHash = passwordHash,
                IsActive = true
            };
            db.Users.Add(user);
        }
        else if (!user.IsActive)
        {
            user.IsActive = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        var rolePermissionIds = await db.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in existingPermissions.Values)
        {
            if (!rolePermissionIds.Contains(permission.Id))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        var hasUserRole = await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, cancellationToken);
        if (!hasUserRole)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        await SeedSampleDataAsync(db, cancellationToken);
        await EnsureExpandedDemoDatasetAsync(db, user.Id, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSampleDataAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var sampleRoot = TryResolveSampleRoot();
        if (!string.IsNullOrWhiteSpace(sampleRoot) && Directory.Exists(sampleRoot))
        {
            await SeedSampleDataFromFoldersAsync(db, sampleRoot, cancellationToken);
            return;
        }

        var hasSampleData = await db.Projects.AnyAsync(x => x.ProjectCode.StartsWith("SGO-SAMPLE-"), cancellationToken);
        if (hasSampleData)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var customerA = new Customer
        {
            FullName = "Rajesh Patil",
            PhoneNumber = "9811100011",
            AlternatePhone = "9811100012",
            Address = "Kothrud",
            City = "Pune",
            State = "Maharashtra",
            PanNumber = "ABCDE1234F",
            BankName = "State Bank of India",
            BankAccountNumber = "210012345678",
            BankIFSC = "SBIN0000456",
            Notes = "Prefers updates every evening."
        };

        var customerB = new Customer
        {
            FullName = "Meera Sharma",
            PhoneNumber = "9822200022",
            Address = "Vaishali Nagar",
            City = "Jaipur",
            State = "Rajasthan",
            PanNumber = "PQRSX6789K",
            BankName = "HDFC Bank",
            BankAccountNumber = "50200098765432",
            BankIFSC = "HDFC0001021",
            Notes = "On-grid system with subsidy follow-up pending."
        };

        db.Customers.AddRange(customerA, customerB);
        await db.SaveChangesAsync(cancellationToken);

        db.CustomerDocuments.AddRange(
            new CustomerDocument
            {
                CustomerId = customerA.Id,
                DocumentType = DocumentType.PAN,
                FileName = "rajesh-patil-pan.pdf",
                FilePath = "sample/customers/rajesh-patil/pan.pdf",
                OriginalFileName = "pan-card.pdf",
                Notes = "Sample PAN document",
                UploadedAtUtc = now.AddDays(-30)
            },
            new CustomerDocument
            {
                CustomerId = customerB.Id,
                DocumentType = DocumentType.BankDetails,
                FileName = "meera-sharma-bank-proof.pdf",
                FilePath = "sample/customers/meera-sharma/bank-proof.pdf",
                OriginalFileName = "bank-proof.pdf",
                Notes = "Sample bank proof",
                UploadedAtUtc = now.AddDays(-20)
            });

        var projectA = new Project
        {
            CustomerId = customerA.Id,
            ProjectCode = "SGO-SAMPLE-PUNE-001",
            CapacityKW = 5.00m,
            CurrentPhase = ProjectPhase.Installation,
            InstallationStartDate = now.AddDays(-5),
            InstallationEndDate = now.AddDays(2),
            SiteAddress = "Kothrud, Pune",
            Notes = "Residential rooftop system."
        };

        var projectB = new Project
        {
            CustomerId = customerB.Id,
            ProjectCode = "SGO-SAMPLE-JPR-001",
            CapacityKW = 8.50m,
            CurrentPhase = ProjectPhase.NetMeter,
            InstallationStartDate = now.AddDays(-18),
            InstallationEndDate = now.AddDays(-12),
            SiteAddress = "Vaishali Nagar, Jaipur",
            Notes = "Commercial unit with three-phase meter."
        };

        db.Projects.AddRange(projectA, projectB);
        await db.SaveChangesAsync(cancellationToken);

        db.ProjectMilestones.AddRange(
            new ProjectMilestone
            {
                ProjectId = projectA.Id,
                Phase = ProjectPhase.Agreement,
                CompletedAtUtc = now.AddDays(-12),
                Notes = "Agreement signed by customer."
            },
            new ProjectMilestone
            {
                ProjectId = projectA.Id,
                Phase = ProjectPhase.Installation,
                CompletedAtUtc = now.AddDays(-2),
                Notes = "Structure and cabling completed."
            },
            new ProjectMilestone
            {
                ProjectId = projectB.Id,
                Phase = ProjectPhase.Installation,
                CompletedAtUtc = now.AddDays(-11),
                Notes = "Panels and inverter commissioned."
            },
            new ProjectMilestone
            {
                ProjectId = projectB.Id,
                Phase = ProjectPhase.DCRFiling,
                CompletedAtUtc = now.AddDays(-8),
                Notes = "DCR filing submitted."
            });

        var sessionA1 = new InstallationSession
        {
            ProjectId = projectA.Id,
            SessionDateUtc = now.AddDays(-4),
            WorkSummary = "Roof mount rails installed.",
            IsCompletedForDay = true
        };
        var sessionA2 = new InstallationSession
        {
            ProjectId = projectA.Id,
            SessionDateUtc = now.AddDays(-2),
            WorkSummary = "Panel wiring and inverter placement done.",
            IsCompletedForDay = true
        };
        var sessionB1 = new InstallationSession
        {
            ProjectId = projectB.Id,
            SessionDateUtc = now.AddDays(-15),
            WorkSummary = "Civil work and frame alignment.",
            IsCompletedForDay = true
        };

        db.InstallationSessions.AddRange(sessionA1, sessionA2, sessionB1);
        await db.SaveChangesAsync(cancellationToken);

        db.InstallationEvidence.AddRange(
            new InstallationEvidence
            {
                InstallationSessionId = sessionA1.Id,
                FilePath = "sample/installations/pune-001/day1-frame.jpg",
                FileName = "day1-frame.jpg",
                MediaType = "Photo",
                CapturedAtUtc = now.AddDays(-4),
                Latitude = 18.5074m,
                Longitude = 73.8077m,
                Notes = "Frame alignment photo"
            },
            new InstallationEvidence
            {
                InstallationSessionId = sessionA2.Id,
                FilePath = "sample/installations/pune-001/day2-wiring.jpg",
                FileName = "day2-wiring.jpg",
                MediaType = "Photo",
                CapturedAtUtc = now.AddDays(-2),
                Latitude = 18.5075m,
                Longitude = 73.8076m,
                Notes = "Wiring completion snapshot"
            },
            new InstallationEvidence
            {
                InstallationSessionId = sessionB1.Id,
                FilePath = "sample/installations/jpr-001/day1-structure.jpg",
                FileName = "day1-structure.jpg",
                MediaType = "Photo",
                CapturedAtUtc = now.AddDays(-15),
                Latitude = 26.9124m,
                Longitude = 75.7873m,
                Notes = "Structure preparation"
            });

        db.PanelAssignments.AddRange(
            new PanelAssignment
            {
                ProjectId = projectA.Id,
                SerialNumber = "SGOPNL-PUNE-001",
                Wattage = 550,
                Brand = "Helios",
                Model = "HX-550",
                AssignedAtUtc = now.AddDays(-3),
                Notes = "South block"
            },
            new PanelAssignment
            {
                ProjectId = projectA.Id,
                SerialNumber = "SGOPNL-PUNE-002",
                Wattage = 550,
                Brand = "Helios",
                Model = "HX-550",
                AssignedAtUtc = now.AddDays(-3),
                Notes = "South block"
            },
            new PanelAssignment
            {
                ProjectId = projectB.Id,
                SerialNumber = "SGOPNL-JPR-001",
                Wattage = 540,
                Brand = "SunPrime",
                Model = "SP-540",
                AssignedAtUtc = now.AddDays(-16),
                Notes = "Main array"
            });

        db.InverterAssignments.AddRange(
            new InverterAssignment
            {
                ProjectId = projectA.Id,
                SerialNumber = "SGOINV-PUNE-001",
                CapacityKva = 5.0m,
                Brand = "VoltEdge",
                Model = "VE-5K",
                AssignedAtUtc = now.AddDays(-2),
                Notes = "Single phase inverter"
            },
            new InverterAssignment
            {
                ProjectId = projectB.Id,
                SerialNumber = "SGOINV-JPR-001",
                CapacityKva = 8.0m,
                Brand = "VoltEdge",
                Model = "VE-8K",
                AssignedAtUtc = now.AddDays(-14),
                Notes = "Three phase inverter"
            });

        db.InvoiceRecords.AddRange(
            new InvoiceRecord
            {
                ProjectId = projectA.Id,
                InvoiceNumber = "INV-SGO-1001",
                Amount = 275000m,
                InvoiceDateUtc = now.AddDays(-1),
                FilePath = "sample/invoices/inv-sgo-1001.pdf",
                Notes = "Advance invoice"
            },
            new InvoiceRecord
            {
                ProjectId = projectB.Id,
                InvoiceNumber = "INV-SGO-1002",
                Amount = 445000m,
                InvoiceDateUtc = now.AddDays(-10),
                FilePath = "sample/invoices/inv-sgo-1002.pdf",
                Notes = "Final invoice"
            });

        db.PaymentReceipts.AddRange(
            new PaymentReceipt
            {
                ProjectId = projectA.Id,
                Amount = 100000m,
                ReceiptDateUtc = now.AddDays(-1),
                PaymentMode = "UPI",
                ReceiptNumber = "RCPT-SGO-2001",
                FilePath = "sample/receipts/rcpt-sgo-2001.pdf",
                Notes = "Booking amount"
            },
            new PaymentReceipt
            {
                ProjectId = projectB.Id,
                Amount = 445000m,
                ReceiptDateUtc = now.AddDays(-9),
                PaymentMode = "BankTransfer",
                ReceiptNumber = "RCPT-SGO-2002",
                FilePath = "sample/receipts/rcpt-sgo-2002.pdf",
                Notes = "Full payment received"
            });
    }

    private static async Task EnsureExpandedDemoDatasetAsync(AppDbContext db, Guid technicianUserId, CancellationToken cancellationToken)
    {
        const int minimumProjectCount = 20;
        var currentCount = await db.Projects.CountAsync(x => !x.IsDeleted, cancellationToken);
        if (currentCount >= minimumProjectCount)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var banks = new[] { "State Bank of India", "HDFC Bank", "ICICI Bank", "Axis Bank", "Punjab National Bank" };
        var brands = new (string Brand, string Model, int Wattage, decimal InverterKva)[]
        {
            ("Waaree", "WSM-550", 550, 5.0m),
            ("Adani Solar", "AS-540", 540, 6.0m),
            ("Vikram Solar", "VS-555", 555, 8.0m),
            ("Tata Power Solar", "TP-500", 500, 4.0m)
        };

        var demoEntries = new (string Name, string City, string State, decimal CapacityKw, ProjectPhase Phase, int PanelCount, int BrandIndex, InstallationClosureStatus ClosureStatus, bool CompletedForDay, int StartedDaysAgo)[]
        {
            ("Rakesh Kumar", "Lucknow", "Uttar Pradesh", 4.0m, ProjectPhase.Installation, 8, 3, InstallationClosureStatus.Open, false, 6),
            ("Sunita Verma", "Nagpur", "Maharashtra", 6.5m, ProjectPhase.NetMeter, 12, 1, InstallationClosureStatus.ClosedApproved, true, 22),
            ("Arvind Iyer", "Chennai", "Tamil Nadu", 10.0m, ProjectPhase.DCRFiling, 18, 2, InstallationClosureStatus.Open, true, 15),
            ("Priya Nair", "Kochi", "Kerala", 3.5m, ProjectPhase.Agreement, 7, 0, InstallationClosureStatus.Open, false, 2),
            ("Manoj Deshmukh", "Nashik", "Maharashtra", 5.0m, ProjectPhase.Installation, 9, 0, InstallationClosureStatus.ClosedPendingApproval, true, 5),
            ("Kavita Joshi", "Indore", "Madhya Pradesh", 7.2m, ProjectPhase.Closed, 13, 1, InstallationClosureStatus.ClosedApproved, true, 40),
            ("Suresh Reddy", "Hyderabad", "Telangana", 12.0m, ProjectPhase.Invoiced, 22, 2, InstallationClosureStatus.ClosedApproved, true, 30),
            ("Anjali Mehta", "Ahmedabad", "Gujarat", 4.5m, ProjectPhase.NetMeter, 8, 0, InstallationClosureStatus.ClosedApproved, true, 20),
            ("Vikram Singh", "Chandigarh", "Punjab", 6.0m, ProjectPhase.Installation, 11, 3, InstallationClosureStatus.ClosedPendingApproval, true, 7),
            ("Deepa Pillai", "Trivandrum", "Kerala", 3.0m, ProjectPhase.Feasibility, 6, 1, InstallationClosureStatus.Open, false, 1),
            ("Amitabh Choudhary", "Patna", "Bihar", 5.5m, ProjectPhase.Agreement, 10, 0, InstallationClosureStatus.Open, false, 3),
            ("Ritu Bansal", "Ludhiana", "Punjab", 9.0m, ProjectPhase.DCRFiling, 16, 2, InstallationClosureStatus.Open, true, 17),
            ("Harish Rao", "Bengaluru", "Karnataka", 8.0m, ProjectPhase.SubsidyProcessed, 15, 1, InstallationClosureStatus.ClosedApproved, true, 35),
            ("Neha Kapoor", "Gurugram", "Haryana", 4.0m, ProjectPhase.Installation, 8, 3, InstallationClosureStatus.ClosedPendingApproval, true, 6),
            ("Sanjay Bhatt", "Surat", "Gujarat", 6.8m, ProjectPhase.NetMeter, 12, 0, InstallationClosureStatus.ClosedApproved, true, 19),
            ("Lakshmi Narayan", "Coimbatore", "Tamil Nadu", 11.0m, ProjectPhase.Invoiced, 20, 2, InstallationClosureStatus.ClosedApproved, true, 28),
            ("Farhan Sheikh", "Bhopal", "Madhya Pradesh", 3.2m, ProjectPhase.Quotation, 6, 1, InstallationClosureStatus.Open, false, 1),
            ("Geeta Rathi", "Jodhpur", "Rajasthan", 5.0m, ProjectPhase.Installation, 9, 0, InstallationClosureStatus.Open, true, 8),
            ("Om Prakash Yadav", "Kanpur", "Uttar Pradesh", 7.5m, ProjectPhase.Closed, 14, 3, InstallationClosureStatus.ClosedApproved, true, 45),
            ("Shalini Menon", "Mysuru", "Karnataka", 4.8m, ProjectPhase.Agreement, 9, 2, InstallationClosureStatus.Open, false, 4)
        };

        var toCreate = Math.Min(demoEntries.Length, minimumProjectCount - currentCount);
        var phoneSeed = 96000_00000L;

        for (var i = 0; i < toCreate; i++)
        {
            var entry = demoEntries[i];
            var brand = brands[entry.BrandIndex];
            var phone = (phoneSeed + (i * 111)).ToString();

            var customer = new Customer
            {
                FullName = entry.Name,
                PhoneNumber = phone,
                Address = $"{entry.City} Main Road",
                City = entry.City,
                State = entry.State,
                PanNumber = $"DEMO{i:0000}PAN",
                BankName = banks[i % banks.Length],
                BankAccountNumber = (100000000000L + (i * 987654)).ToString(),
                BankIFSC = $"{banks[i % banks.Length][..4].ToUpperInvariant()}0{1000 + i}",
                Notes = "Demo record for dashboard and reporting walkthroughs."
            };
            db.Customers.Add(customer);
            await db.SaveChangesAsync(cancellationToken);

            var project = new Project
            {
                CustomerId = customer.Id,
                ProjectCode = $"SGO-DEMO-{(i + 1):000}",
                CapacityKW = entry.CapacityKw,
                CurrentPhase = entry.Phase,
                InstallationStartDate = now.AddDays(-entry.StartedDaysAgo),
                InstallationEndDate = entry.Phase == ProjectPhase.Closed ? now.AddDays(-entry.StartedDaysAgo + 10) : null,
                SiteAddress = $"{entry.City}, {entry.State}",
                Notes = "Seeded demo project with realistic values for pilot walkthroughs."
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync(cancellationToken);

            var panelAssignments = new List<PanelAssignment>();
            for (var p = 0; p < entry.PanelCount; p++)
            {
                panelAssignments.Add(new PanelAssignment
                {
                    ProjectId = project.Id,
                    SerialNumber = $"SGOPNL-DEMO-{(i + 1):000}-{(p + 1):00}",
                    Wattage = brand.Wattage,
                    Brand = brand.Brand,
                    Model = brand.Model,
                    AssignedAtUtc = now.AddDays(-entry.StartedDaysAgo),
                    Notes = "Demo panel assignment"
                });
            }
            db.PanelAssignments.AddRange(panelAssignments);

            db.InverterAssignments.Add(new InverterAssignment
            {
                ProjectId = project.Id,
                SerialNumber = $"SGOINV-DEMO-{(i + 1):000}",
                CapacityKva = brand.InverterKva,
                Brand = brand.Brand,
                Model = $"{brand.Model}-INV",
                AssignedAtUtc = now.AddDays(-entry.StartedDaysAgo),
                Notes = "Demo inverter assignment"
            });

            var session = new InstallationSession
            {
                ProjectId = project.Id,
                TechnicianUserId = technicianUserId,
                SessionDateUtc = now.AddDays(-Math.Max(entry.StartedDaysAgo - 1, 0)),
                WorkSummary = $"Installation progress recorded for {entry.Name}'s {entry.CapacityKw}kW system.",
                IsCompletedForDay = entry.CompletedForDay,
                ClosureStatus = entry.ClosureStatus,
                ClosureRequestedAtUtc = entry.ClosureStatus != InstallationClosureStatus.Open ? now.AddDays(-1) : null,
                ClosureApprovedAtUtc = entry.ClosureStatus == InstallationClosureStatus.ClosedApproved ? now : null
            };
            db.InstallationSessions.Add(session);

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedSampleDataFromFoldersAsync(AppDbContext db, string sampleRoot, CancellationToken cancellationToken)
    {
        var projectCode = "SGO-SAMPLE-DURGESH-001";
        var exists = await db.Projects.AnyAsync(x => x.ProjectCode == projectCode, cancellationToken);
        if (exists)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var durgeshFolder = FindNestedFolder(sampleRoot, "Durgesh Gaju");
        if (string.IsNullOrWhiteSpace(durgeshFolder) || !Directory.Exists(durgeshFolder))
        {
            return;
        }

        var customer = new Customer
        {
            FullName = "Durgesh Gaju",
            PhoneNumber = "9898989898",
            AlternatePhone = "9898989800",
            Address = "Raya",
            City = "Mathura",
            State = "Uttar Pradesh",
            PanNumber = "DURGESH-PAN",
            BankName = "Sample Bank",
            BankAccountNumber = "000000000000",
            BankIFSC = "SAMP0000001",
            Notes = "Seeded from local sample folder data/Sample."
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        var project = new Project
        {
            CustomerId = customer.Id,
            ProjectCode = projectCode,
            CapacityKW = 3.00m,
            CurrentPhase = ProjectPhase.Installation,
            InstallationStartDate = now.AddDays(-30),
            InstallationEndDate = now.AddDays(2),
            SiteAddress = "Raya, Mathura",
            Notes = "Project seeded from direct sample files provided by client."
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        var session = new InstallationSession
        {
            ProjectId = project.Id,
            SessionDateUtc = now.AddDays(-1),
            WorkSummary = "Sample first-run installation session seeded from folder files.",
            IsCompletedForDay = true
        };
        db.InstallationSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        var files = Directory.GetFiles(durgeshFolder, "*", SearchOption.TopDirectoryOnly)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var images = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var documentExt = new[] { ".pdf", ".doc", ".docx", ".xlsx", ".xls", ".jpg", ".jpeg", ".png" };

        var customerDocuments = new List<CustomerDocument>();
        var evidenceItems = new List<InstallationEvidence>();
        var invoices = new List<InvoiceRecord>();
        var receipts = new List<PaymentReceipt>();

        var invoiceIndex = 1;
        var receiptIndex = 1;

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);
            var fileName = Path.GetFileName(file);
            var relativePath = NormalizeRelativePath(file);
            var inferredType = InferDocumentType(fileName);

            if (documentExt.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                customerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = inferredType,
                    FileName = TrimTo(fileName, 250),
                    OriginalFileName = TrimTo(fileName, 250),
                    FilePath = TrimTo(relativePath, 500),
                    Notes = "Imported from local sample folder",
                    UploadedAtUtc = now.AddMinutes(-invoiceIndex)
                });
            }

            if (images.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                evidenceItems.Add(new InstallationEvidence
                {
                    InstallationSessionId = session.Id,
                    FilePath = TrimTo(relativePath, 500),
                    FileName = TrimTo(fileName, 250),
                    MediaType = "Photo",
                    CapturedAtUtc = now.AddMinutes(-receiptIndex),
                    Notes = "Imported from local sample folder"
                });
            }

            if (fileName.Contains("invoice", StringComparison.OrdinalIgnoreCase))
            {
                invoices.Add(new InvoiceRecord
                {
                    ProjectId = project.Id,
                    InvoiceNumber = TrimTo($"INV-DURG-{invoiceIndex:000}", 60),
                    Amount = 0m,
                    InvoiceDateUtc = now.AddDays(-invoiceIndex),
                    FilePath = TrimTo(relativePath, 500),
                    Notes = "Imported from local sample folder"
                });
                invoiceIndex++;
            }

            if (fileName.Contains("receipt", StringComparison.OrdinalIgnoreCase) || fileName.Contains("ackn", StringComparison.OrdinalIgnoreCase))
            {
                receipts.Add(new PaymentReceipt
                {
                    ProjectId = project.Id,
                    Amount = 0m,
                    ReceiptDateUtc = now.AddDays(-receiptIndex),
                    PaymentMode = "Unknown",
                    ReceiptNumber = TrimTo($"RCPT-DURG-{receiptIndex:000}", 60),
                    FilePath = TrimTo(relativePath, 500),
                    Notes = "Imported from local sample folder"
                });
                receiptIndex++;
            }
        }

        if (customerDocuments.Count > 0)
        {
            db.CustomerDocuments.AddRange(customerDocuments);
        }

        if (evidenceItems.Count > 0)
        {
            db.InstallationEvidence.AddRange(evidenceItems);
        }

        if (invoices.Count > 0)
        {
            db.InvoiceRecords.AddRange(invoices);
        }

        if (receipts.Count > 0)
        {
            db.PaymentReceipts.AddRange(receipts);
        }

        var panelsFolder = FindNestedFolder(sampleRoot, "PANELS DETAILS OF CUSTOMERS");
        if (!string.IsNullOrWhiteSpace(panelsFolder) && Directory.Exists(panelsFolder))
        {
            var panelFiles = Directory.GetFiles(panelsFolder, "*.xlsx", SearchOption.TopDirectoryOnly)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Take(40)
                .ToList();

            var panelAssignments = new List<PanelAssignment>();
            var inverterAssignments = new List<InverterAssignment>();
            var serialIndex = 1;

            foreach (var panelFile in panelFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(panelFile);
                var normalizedName = NormalizeNameFromFileName(fileName);

                if (fileName.Contains("inverter", StringComparison.OrdinalIgnoreCase))
                {
                    inverterAssignments.Add(new InverterAssignment
                    {
                        ProjectId = project.Id,
                        SerialNumber = TrimTo($"INV-SAMPLE-{serialIndex:000}", 100),
                        CapacityKva = 0m,
                        Brand = "Unknown",
                        Model = TrimTo(normalizedName, 100),
                        AssignedAtUtc = now.AddDays(-serialIndex),
                        Notes = "From panel detail workbook file name"
                    });
                }
                else
                {
                    panelAssignments.Add(new PanelAssignment
                    {
                        ProjectId = project.Id,
                        SerialNumber = TrimTo($"PNL-SAMPLE-{serialIndex:000}", 100),
                        Wattage = 0,
                        Brand = "Unknown",
                        Model = TrimTo(normalizedName, 100),
                        AssignedAtUtc = now.AddDays(-serialIndex),
                        Notes = "From panel detail workbook file name"
                    });
                }

                serialIndex++;
            }

            if (panelAssignments.Count > 0)
            {
                db.PanelAssignments.AddRange(panelAssignments);
            }

            if (inverterAssignments.Count > 0)
            {
                db.InverterAssignments.AddRange(inverterAssignments);
            }
        }
    }

    private static string? TryResolveSampleRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "data", "Sample");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private static string? FindNestedFolder(string root, string folderName)
    {
        var direct = Path.Combine(root, folderName);
        if (!Directory.Exists(direct))
        {
            return null;
        }

        var nested = Path.Combine(direct, folderName);
        return Directory.Exists(nested) ? nested : direct;
    }

    private static DocumentType InferDocumentType(string fileName)
    {
        if (fileName.Contains("pan", StringComparison.OrdinalIgnoreCase)) return DocumentType.PAN;
        if (fileName.Contains("bank", StringComparison.OrdinalIgnoreCase)) return DocumentType.BankDetails;
        if (fileName.Contains("agreement", StringComparison.OrdinalIgnoreCase)) return DocumentType.UserAgreement;
        if (fileName.Contains("approval", StringComparison.OrdinalIgnoreCase)) return DocumentType.DigitalApproval;
        if (fileName.Contains("quotation", StringComparison.OrdinalIgnoreCase)) return DocumentType.Quotation;
        if (fileName.Contains("feasibility", StringComparison.OrdinalIgnoreCase)) return DocumentType.Feasibility;
        if (fileName.Contains("invoice", StringComparison.OrdinalIgnoreCase)) return DocumentType.TaxInvoice;
        if (fileName.Contains("dcr", StringComparison.OrdinalIgnoreCase)) return DocumentType.DCRUndertaking;
        if (fileName.Contains("netmeter", StringComparison.OrdinalIgnoreCase)) return DocumentType.NetMeter;
        if (fileName.Contains("subsidy", StringComparison.OrdinalIgnoreCase)) return DocumentType.Subsidy;
        if (fileName.Contains("receipt", StringComparison.OrdinalIgnoreCase)) return DocumentType.CashReceipt;
        if (fileName.Contains("gps", StringComparison.OrdinalIgnoreCase)) return DocumentType.GPSPhoto;
        if (fileName.Contains("ackn", StringComparison.OrdinalIgnoreCase)) return DocumentType.Acknowledgment;
        return DocumentType.Other;
    }

    private static string NormalizeRelativePath(string fullPath)
    {
        var cwd = Directory.GetCurrentDirectory();
        var relative = Path.GetRelativePath(cwd, fullPath);
        return relative.Replace('\\', '/');
    }

    private static string NormalizeNameFromFileName(string raw)
    {
        var cleaned = Regex.Replace(raw, "\\s+", " ").Trim();
        cleaned = cleaned.Replace("PANELS DETAILS", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("PANELS S.NO.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("INVERTER Sr.No.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim(' ', '-', '_', '.');

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            cleaned = "SampleModel";
        }

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
    }

    private static string TrimTo(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
