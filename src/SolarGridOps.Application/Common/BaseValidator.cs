using FluentValidation;

namespace SolarGridOps.Application.Common;

// Base class all request validators inherit from — enforces consistent validation style
public abstract class BaseValidator<T> : AbstractValidator<T>
{
    protected BaseValidator()
    {
        // cascade stops validating a property after the first rule fails
        ClassLevelCascadeMode = CascadeMode.Stop;
    }
}
