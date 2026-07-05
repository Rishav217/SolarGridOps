using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Application.Features.Auth;
using SolarGridOps.Application.Features.Customers;
using SolarGridOps.Application.Features.Installations;
using SolarGridOps.Application.Features.Projects;

namespace SolarGridOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInstallationService, InstallationService>();
        return services;
    }
}
