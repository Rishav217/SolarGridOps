using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Inventory;

public class CreatePanelInventoryRequestValidator : BaseValidator<CreatePanelInventoryRequest>
{
    public CreatePanelInventoryRequestValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Wattage).GreaterThan(0).LessThanOrEqualTo(1000);
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
