using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Inventory;

public interface IInventoryService
{
    Task<Result<PanelInventoryDto>> AddPanelAsync(Guid projectId, CreatePanelInventoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PanelInventoryDto>>> ListPanelsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<PanelInventoryDto>> UpdatePanelAsync(Guid panelId, UpdatePanelInventoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemovePanelAsync(Guid panelId, CancellationToken cancellationToken = default);
    Task<Result<InventoryMovementDto>> RecordMovementAsync(CreateInventoryMovementRequest request, Guid? recordedByUserId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventoryMovementDto>>> ListMovementsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventoryMovementDto>>> ListPanelMovementsAsync(Guid panelId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventoryMovementDto>>> ListInverterMovementsAsync(Guid inverterId, CancellationToken cancellationToken = default);
}
