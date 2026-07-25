using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Customers;

public interface ICustomerService
{
    Task<Result<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CustomerDto>>> ListAsync(CancellationToken cancellationToken = default);
}
