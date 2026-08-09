using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Installations;

public class CreateInstallationSessionRequestValidator : BaseValidator<CreateInstallationSessionRequest>
{
    public CreateInstallationSessionRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.WorkSummary).MaximumLength(1000);
    }
}
