using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Application.Features.Auth;
using SolarGridOps.Application.Features.Customers;
using SolarGridOps.Application.Features.Installations;

namespace SolarGridOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInstallationService, InstallationService>();
        return services;
    }
}
