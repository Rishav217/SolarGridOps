using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<PanelInventoryDto>> AddPanelAsync(Guid projectId, CreatePanelInventoryRequest request, CancellationToken cancellationToken = default)
    {
        var projectExists = await _inventoryRepository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result<PanelInventoryDto>.Failure(Error.NotFound("Project not found."));
        }

        var serial = request.SerialNumber.Trim();
        var serialExists = await _inventoryRepository.SerialNumberExistsAsync(serial, cancellationToken);
        if (serialExists)
        {
            return Result<PanelInventoryDto>.Failure(Error.Conflict("Panel serial number already exists."));
        }

        var panel = new PanelAssignment
        {
            ProjectId = projectId,
            SerialNumber = serial,
            Wattage = request.Wattage,
            Brand = request.Brand?.Trim(),
            Model = request.Model?.Trim(),
            Notes = request.Notes?.Trim(),
            AssignedAtUtc = DateTime.UtcNow
        };

        await _inventoryRepository.AddPanelAsync(panel, cancellationToken);

        return Result<PanelInventoryDto>.Success(Map(panel));
    }

    public async Task<Result<IReadOnlyList<PanelInventoryDto>>> ListPanelsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _inventoryRepository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result<IReadOnlyList<PanelInventoryDto>>.Failure(Error.NotFound("Project not found."));
        }

        var panels = await _inventoryRepository.ListPanelsByProjectAsync(projectId, cancellationToken);
        return Result<IReadOnlyList<PanelInventoryDto>>.Success(panels.Select(Map).ToList());
    }

    private static PanelInventoryDto Map(PanelAssignment panel)
    {
        return new PanelInventoryDto
        {
            Id = panel.Id,
            ProjectId = panel.ProjectId,
            SerialNumber = panel.SerialNumber,
            Wattage = panel.Wattage,
            Brand = panel.Brand,
            Model = panel.Model,
            AssignedAtUtc = panel.AssignedAtUtc,
            Notes = panel.Notes
        };
    }
}
