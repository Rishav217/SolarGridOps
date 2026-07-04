using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Application.Features.Auth;
using SolarGridOps.Application.Features.Customers;
using SolarGridOps.Application.Features.Installations;
using SolarGridOps.Infrastructure.Persistence;
using SolarGridOps.Infrastructure.Persistence.Repositories;
using SolarGridOps.Infrastructure.Security;

namespace SolarGridOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3);
            });
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IInstallationRepository, InstallationRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
