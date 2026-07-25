using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Customers;

public class CreateCustomerRequestValidator : BaseValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AlternatePhone).MaximumLength(20);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PanNumber).MaximumLength(20);
        RuleFor(x => x.BankAccountNumber).MaximumLength(40);
        RuleFor(x => x.BankName).MaximumLength(120);
        RuleFor(x => x.BankIFSC).MaximumLength(20);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
