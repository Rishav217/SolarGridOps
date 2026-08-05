using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Inventory;

public interface IInverterInventoryRepository
{
    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task AddInverterAsync(InverterAssignment inverter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InverterAssignment>> ListInvertersByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
