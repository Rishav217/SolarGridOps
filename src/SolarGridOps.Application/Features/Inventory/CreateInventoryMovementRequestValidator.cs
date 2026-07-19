using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Inventory;

public class CreateInventoryMovementRequestValidator : BaseValidator<CreateInventoryMovementRequest>
{
    private static readonly string[] AllowedItemTypes = ["panel", "inverter"];
    private static readonly string[] AllowedMovementTypes = ["in", "out", "transfer", "adjustment"];

    public CreateInventoryMovementRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();

        RuleFor(x => x.ItemType)
            .NotEmpty()
            .Must(x => AllowedItemTypes.Contains(x.Trim().ToLowerInvariant()))
            .WithMessage("ItemType must be one of: panel, inverter.");

        RuleFor(x => x.MovementType)
            .NotEmpty()
            .Must(x => AllowedMovementTypes.Contains(x.Trim().ToLowerInvariant()))
            .WithMessage("MovementType must be one of: in, out, transfer, adjustment.");

        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCostPrice).GreaterThanOrEqualTo(0).When(x => x.UnitCostPrice.HasValue);
        RuleFor(x => x.UnitSellPrice).GreaterThanOrEqualTo(0).When(x => x.UnitSellPrice.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
