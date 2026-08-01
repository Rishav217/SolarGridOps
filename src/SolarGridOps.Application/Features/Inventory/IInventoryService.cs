using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Inventory;

public interface IInventoryService
{
    Task<Result<PanelInventoryDto>> AddPanelAsync(Guid projectId, CreatePanelInventoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PanelInventoryDto>>> ListPanelsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<PanelInventoryDto>> UpdatePanelAsync(Guid panelId, UpdatePanelInventoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemovePanelAsync(Guid panelId, CancellationToken cancellationToken = default);
}
