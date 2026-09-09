namespace SolarGridOps.Api.Security;

public static class PermissionPolicies
{
    public const string CustomersRead = "customers.read";
    public const string CustomersCreate = "customers.create";

    public const string ProjectsRead = "projects.read";
    public const string ProjectsCreate = "projects.create";
    public const string ProjectsUpdatePhase = "projects.updatePhase";

    public const string InstallationsSessionsRead = "installations.sessions.read";
    public const string InstallationsSessionsCreate = "installations.sessions.create";
    public const string InstallationsSessionsUpdate = "installations.sessions.update";
    public const string InstallationsEvidenceRead = "installations.evidence.read";
    public const string InstallationsEvidenceCreate = "installations.evidence.create";
    public const string InstallationsClosureRequest = "installations.closure.request";
    public const string InstallationsClosureApprove = "installations.closure.approve";

    public const string InventoryPanelsRead = "inventory.panels.read";
    public const string InventoryPanelsCreate = "inventory.panels.create";
    public const string InventoryPanelsUpdate = "inventory.panels.update";
    public const string InventoryPanelsDelete = "inventory.panels.delete";

    public const string InventoryInvertersRead = "inventory.inverters.read";
    public const string InventoryInvertersCreate = "inventory.inverters.create";

    public const string InventoryMovementsRead = "inventory.movements.read";
    public const string InventoryMovementsCreate = "inventory.movements.create";

    public const string AuditTrailRead = "audit.trail.read";

    public const string AuthCapabilities = "auth.capabilities";
}
