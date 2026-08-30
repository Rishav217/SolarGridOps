using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Inventory;

public class InventoryService : IInventoryService
{
    private const string PanelItemType = "panel";
    private const string InverterItemType = "inverter";
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

    public async Task<Result<InventoryMovementDto>> RecordMovementAsync(CreateInventoryMovementRequest request, Guid? recordedByUserId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _inventoryRepository.ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return Result<InventoryMovementDto>.Failure(Error.NotFound("Project not found."));
        }

        var itemType = request.ItemType.Trim().ToLowerInvariant();
        if (itemType != PanelItemType && itemType != InverterItemType)
        {
            return Result<InventoryMovementDto>.Failure(Error.Validation("Unsupported item type."));
        }

        var movementType = request.MovementType.Trim().ToLowerInvariant();
        var movementTypeAllowed = movementType is "in" or "out" or "transfer" or "adjustment";
        if (!movementTypeAllowed)
        {
            return Result<InventoryMovementDto>.Failure(Error.Validation("Unsupported movement type."));
        }

        var itemExists = itemType == PanelItemType
            ? await _inventoryRepository.PanelExistsInProjectAsync(request.ItemId, request.ProjectId, cancellationToken)
            : await _inventoryRepository.InverterExistsInProjectAsync(request.ItemId, request.ProjectId, cancellationToken);

        if (!itemExists)
        {
            return Result<InventoryMovementDto>.Failure(Error.NotFound("Inventory item not found in project."));
        }

        var movement = new InventoryMovement
        {
            ProjectId = request.ProjectId,
            ItemId = request.ItemId,
            ItemType = itemType,
            MovementType = movementType,
            Quantity = request.Quantity,
            UnitCostPrice = request.UnitCostPrice,
            UnitSellPrice = request.UnitSellPrice,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RecordedByUserId = recordedByUserId,
            MovedAtUtc = DateTime.UtcNow
        };

        await _inventoryRepository.AddMovementAsync(movement, cancellationToken);
        return Result<InventoryMovementDto>.Success(Map(movement));
    }

    public async Task<Result<IReadOnlyList<InventoryMovementDto>>> ListMovementsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _inventoryRepository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result<IReadOnlyList<InventoryMovementDto>>.Failure(Error.NotFound("Project not found."));
        }

        var movements = await _inventoryRepository.ListMovementsByProjectAsync(projectId, cancellationToken);
        return Result<IReadOnlyList<InventoryMovementDto>>.Success(movements.Select(Map).ToList());
    }

    public async Task<Result<IReadOnlyList<InventoryMovementDto>>> ListPanelMovementsAsync(Guid panelId, CancellationToken cancellationToken = default)
    {
        var panel = await _inventoryRepository.GetPanelByIdAsync(panelId, cancellationToken);
        if (panel is null)
        {
            return Result<IReadOnlyList<InventoryMovementDto>>.Failure(Error.NotFound("Panel not found."));
        }

        var movements = await _inventoryRepository.ListMovementsByItemAsync(PanelItemType, panelId, cancellationToken);
        return Result<IReadOnlyList<InventoryMovementDto>>.Success(movements.Select(Map).ToList());
    }

    public async Task<Result<IReadOnlyList<InventoryMovementDto>>> ListInverterMovementsAsync(Guid inverterId, CancellationToken cancellationToken = default)
    {
        var inverter = await _inventoryRepository.GetInverterByIdAsync(inverterId, cancellationToken);
        if (inverter is null)
        {
            return Result<IReadOnlyList<InventoryMovementDto>>.Failure(Error.NotFound("Inverter not found."));
        }

        var movements = await _inventoryRepository.ListMovementsByItemAsync(InverterItemType, inverterId, cancellationToken);
        return Result<IReadOnlyList<InventoryMovementDto>>.Success(movements.Select(Map).ToList());
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

    private static InventoryMovementDto Map(InventoryMovement movement)
    {
        return new InventoryMovementDto
        {
            Id = movement.Id,
            ProjectId = movement.ProjectId,
            ItemId = movement.ItemId,
            ItemType = movement.ItemType,
            MovementType = movement.MovementType,
            Quantity = movement.Quantity,
            UnitCostPrice = movement.UnitCostPrice,
            UnitSellPrice = movement.UnitSellPrice,
            Notes = movement.Notes,
            RecordedByUserId = movement.RecordedByUserId,
            MovedAtUtc = movement.MovedAtUtc
        };
    }
}
