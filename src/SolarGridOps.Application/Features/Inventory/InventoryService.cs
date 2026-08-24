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

    public async Task<Result<PanelInventoryDto>> UpdatePanelAsync(Guid panelId, UpdatePanelInventoryRequest request, CancellationToken cancellationToken = default)
    {
        var panel = await _inventoryRepository.GetPanelByIdAsync(panelId, cancellationToken);
        if (panel is null)
        {
            return Result<PanelInventoryDto>.Failure(Error.NotFound("Panel not found."));
        }

        if (request.SerialNumber is not null)
        {
            var nextSerial = request.SerialNumber.Trim();
            var serialInUse = await _inventoryRepository.SerialNumberExistsForOtherPanelAsync(panelId, nextSerial, cancellationToken);
            if (serialInUse)
            {
                return Result<PanelInventoryDto>.Failure(Error.Conflict("Panel serial number already exists."));
            }

            panel.SerialNumber = nextSerial;
        }

        if (request.Wattage.HasValue)
        {
            panel.Wattage = request.Wattage.Value;
        }

        if (request.Brand is not null)
        {
            panel.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
        }

        if (request.Model is not null)
        {
            panel.Model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim();
        }

        if (request.Notes is not null)
        {
            panel.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        await _inventoryRepository.SaveChangesAsync(cancellationToken);
        return Result<PanelInventoryDto>.Success(Map(panel));
    }

    public async Task<Result> RemovePanelAsync(Guid panelId, CancellationToken cancellationToken = default)
    {
        var panel = await _inventoryRepository.GetPanelByIdAsync(panelId, cancellationToken);
        if (panel is null)
        {
            return Result.Failure(Error.NotFound("Panel not found."));
        }

        panel.IsDeleted = true;
        await _inventoryRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
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
