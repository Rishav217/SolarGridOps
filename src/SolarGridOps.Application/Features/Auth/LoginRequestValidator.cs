using FluentValidation;
using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Auth;

public class LoginRequestValidator : BaseValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UsernameOrMobile).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
