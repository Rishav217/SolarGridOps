using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Inventory;

public class UpdatePanelInventoryRequestValidator : BaseValidator<UpdatePanelInventoryRequest>
{
    public UpdatePanelInventoryRequestValidator()
    {
        RuleFor(x => x.SerialNumber)
            .MaximumLength(100)
            .When(x => x.SerialNumber is not null);

        RuleFor(x => x.Wattage)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .When(x => x.Wattage.HasValue);

        RuleFor(x => x.Brand)
            .MaximumLength(100)
            .When(x => x.Brand is not null);

        RuleFor(x => x.Model)
            .MaximumLength(100)
            .When(x => x.Model is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);

        RuleFor(x => x)
            .Must(x =>
                x.SerialNumber is not null ||
                x.Wattage.HasValue ||
                x.Brand is not null ||
                x.Model is not null ||
                x.Notes is not null)
            .WithMessage("At least one field is required for update.");
    }
}
