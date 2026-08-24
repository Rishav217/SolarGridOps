using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var phone = request.PhoneNumber.Trim();
        var exists = await _customerRepository.PhoneExistsAsync(phone, cancellationToken);
        if (exists)
        {
            return Result<CustomerDto>.Failure(Error.Conflict("Customer with same phone number already exists."));
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = phone,
            AlternatePhone = request.AlternatePhone?.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PanNumber = request.PanNumber?.Trim(),
            BankAccountNumber = request.BankAccountNumber?.Trim(),
            BankName = request.BankName?.Trim(),
            BankIFSC = request.BankIFSC?.Trim(),
            Notes = request.Notes?.Trim()
        };

        await _customerRepository.AddAsync(customer, cancellationToken);

        return Result<CustomerDto>.Success(Map(customer));
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure(Error.NotFound("Customer not found."));
        }

        return Result<CustomerDto>.Success(Map(customer));
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.ListAsync(cancellationToken);
        var data = customers.Select(Map).ToList();
        return Result<IReadOnlyList<CustomerDto>>.Success(data);
    }

    private static CustomerDto Map(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            AlternatePhone = customer.AlternatePhone,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PanNumber = customer.PanNumber,
            BankAccountNumber = customer.BankAccountNumber,
            BankName = customer.BankName,
            BankIFSC = customer.BankIFSC,
            Notes = customer.Notes
        };
    }
}
