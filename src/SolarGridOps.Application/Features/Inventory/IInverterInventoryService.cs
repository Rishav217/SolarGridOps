using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Inventory;

public interface IInverterInventoryService
{
    Task<Result<InverterInventoryDto>> AddInverterAsync(Guid projectId, CreateInverterInventoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InverterInventoryDto>>> ListInvertersByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
