using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Projects;

public class CreateProjectRequestValidator : BaseValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProjectCode).NotEmpty().MaximumLength(40);
        RuleFor(x => x.CapacityKW).GreaterThan(0).LessThanOrEqualTo(10000);
        RuleFor(x => x.SiteAddress).MaximumLength(250);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.InstallationEndDate)
            .GreaterThanOrEqualTo(x => x.InstallationStartDate!.Value)
            .When(x => x.InstallationStartDate.HasValue && x.InstallationEndDate.HasValue);
    }
}
