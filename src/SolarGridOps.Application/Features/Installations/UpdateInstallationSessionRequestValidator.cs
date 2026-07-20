using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Installations;

public class UpdateInstallationSessionRequestValidator : BaseValidator<UpdateInstallationSessionRequest>
{
    public UpdateInstallationSessionRequestValidator()
    {
        RuleFor(x => x.WorkSummary).MaximumLength(1000);
    }
}
