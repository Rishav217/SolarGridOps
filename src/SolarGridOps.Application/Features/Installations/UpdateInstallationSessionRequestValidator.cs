using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Installations;

public class UpdateInstallationSessionRequestValidator : BaseValidator<UpdateInstallationSessionRequest>
{
    public UpdateInstallationSessionRequestValidator()
    {
        RuleFor(x => x.WorkSummary).MaximumLength(1000);

        RuleFor(x => x)
            .Must(x =>
                x.TechnicianUserId.HasValue ||
                x.SessionDateUtc.HasValue ||
                x.WorkSummary is not null ||
                x.IsCompletedForDay.HasValue)
            .WithMessage("At least one field is required for update.");
    }
}
