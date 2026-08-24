using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Inventory;

public interface IInventoryRepository
{
    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task AddPanelAsync(PanelAssignment panel, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PanelAssignment>> ListPanelsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<PanelAssignment?> GetPanelByIdAsync(Guid panelId, CancellationToken cancellationToken = default);
    Task<bool> SerialNumberExistsForOtherPanelAsync(Guid panelId, string serialNumber, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
