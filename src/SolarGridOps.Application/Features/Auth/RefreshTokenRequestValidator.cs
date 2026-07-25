using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Auth;

public class RefreshTokenRequestValidator : BaseValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(300);
    }
}
