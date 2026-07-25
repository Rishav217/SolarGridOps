using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Customers;

public interface ICustomerRepository
{
    Task<bool> PhoneExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken = default);
}
