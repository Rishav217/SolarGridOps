using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Inventory;

public class InverterInventoryService : IInverterInventoryService
{
    private readonly IInverterInventoryRepository _repository;

    public InverterInventoryService(IInverterInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<InverterInventoryDto>> AddInverterAsync(Guid projectId, CreateInverterInventoryRequest request, CancellationToken cancellationToken = default)
    {
        var projectExists = await _repository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result<InverterInventoryDto>.Failure(Error.NotFound("Project not found."));
        }

        var serial = request.SerialNumber.Trim();
        var serialExists = await _repository.SerialNumberExistsAsync(serial, cancellationToken);
        if (serialExists)
        {
            return Result<InverterInventoryDto>.Failure(Error.Conflict("Inverter serial number already exists."));
        }

        var inverter = new InverterAssignment
        {
            ProjectId = projectId,
            SerialNumber = serial,
            CapacityKva = request.CapacityKva,
            Brand = request.Brand?.Trim(),
            Model = request.Model?.Trim(),
            Notes = request.Notes?.Trim(),
            AssignedAtUtc = DateTime.UtcNow
        };

        await _repository.AddInverterAsync(inverter, cancellationToken);
        return Result<InverterInventoryDto>.Success(Map(inverter));
    }

    public async Task<Result<IReadOnlyList<InverterInventoryDto>>> ListInvertersByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _repository.ProjectExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result<IReadOnlyList<InverterInventoryDto>>.Failure(Error.NotFound("Project not found."));
        }

        var inverters = await _repository.ListInvertersByProjectAsync(projectId, cancellationToken);
        return Result<IReadOnlyList<InverterInventoryDto>>.Success(inverters.Select(Map).ToList());
    }

    private static InverterInventoryDto Map(InverterAssignment inverter)
    {
        return new InverterInventoryDto
        {
            Id = inverter.Id,
            ProjectId = inverter.ProjectId,
            SerialNumber = inverter.SerialNumber,
            CapacityKva = inverter.CapacityKva,
            Brand = inverter.Brand,
            Model = inverter.Model,
            AssignedAtUtc = inverter.AssignedAtUtc,
            Notes = inverter.Notes
        };
    }
}
