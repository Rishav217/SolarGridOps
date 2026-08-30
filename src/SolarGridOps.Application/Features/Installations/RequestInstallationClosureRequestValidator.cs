using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Installations;

public class RequestInstallationClosureRequestValidator : BaseValidator<RequestInstallationClosureRequest>
{
    public RequestInstallationClosureRequestValidator()
    {
        RuleFor(x => x.CustomerSignatureName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.CustomerSignatureBase64)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);
    }
}