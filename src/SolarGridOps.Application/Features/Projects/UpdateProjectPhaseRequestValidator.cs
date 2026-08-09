using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Projects;

public class UpdateProjectPhaseRequestValidator : BaseValidator<UpdateProjectPhaseRequest>
{
    public UpdateProjectPhaseRequestValidator()
    {
        RuleFor(x => x.CurrentPhase)
            .IsInEnum();

        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.InstallationEndDate)
            .GreaterThanOrEqualTo(x => x.InstallationStartDate!.Value)
            .When(x => x.InstallationStartDate.HasValue && x.InstallationEndDate.HasValue);
    }
}
