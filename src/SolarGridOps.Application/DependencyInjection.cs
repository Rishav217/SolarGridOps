using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Application.Features.Auth;
using SolarGridOps.Application.Features.Customers;

namespace SolarGridOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
