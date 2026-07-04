using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Installations;

public class AddInstallationEvidenceRequestValidator : BaseValidator<AddInstallationEvidenceRequest>
{
    public AddInstallationEvidenceRequestValidator()
    {
        RuleFor(x => x.FilePath).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.MediaType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
