using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SolarGridOps.Api.Security;
using SolarGridOps.Infrastructure.Security;

namespace SolarGridOps.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Secret))
        {
            throw new InvalidOperationException("Jwt:Secret is missing from configuration.");
        }

        var key = Encoding.UTF8.GetBytes(jwt.Secret);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.FallbackPolicy = options.DefaultPolicy;

            options.AddPolicy(PermissionPolicies.CustomersRead, p => p.RequireClaim("perm", PermissionPolicies.CustomersRead));
            options.AddPolicy(PermissionPolicies.CustomersCreate, p => p.RequireClaim("perm", PermissionPolicies.CustomersCreate));

            options.AddPolicy(PermissionPolicies.ProjectsRead, p => p.RequireClaim("perm", PermissionPolicies.ProjectsRead));
            options.AddPolicy(PermissionPolicies.ProjectsCreate, p => p.RequireClaim("perm", PermissionPolicies.ProjectsCreate));
            options.AddPolicy(PermissionPolicies.ProjectsUpdatePhase, p => p.RequireClaim("perm", PermissionPolicies.ProjectsUpdatePhase));

            options.AddPolicy(PermissionPolicies.InstallationsSessionsRead, p => p.RequireClaim("perm", PermissionPolicies.InstallationsSessionsRead));
            options.AddPolicy(PermissionPolicies.InstallationsSessionsCreate, p => p.RequireClaim("perm", PermissionPolicies.InstallationsSessionsCreate));
            options.AddPolicy(PermissionPolicies.InstallationsSessionsUpdate, p => p.RequireClaim("perm", PermissionPolicies.InstallationsSessionsUpdate));
            options.AddPolicy(PermissionPolicies.InstallationsEvidenceRead, p => p.RequireClaim("perm", PermissionPolicies.InstallationsEvidenceRead));
            options.AddPolicy(PermissionPolicies.InstallationsEvidenceCreate, p => p.RequireClaim("perm", PermissionPolicies.InstallationsEvidenceCreate));
            options.AddPolicy(PermissionPolicies.InstallationsClosureRequest, p => p.RequireClaim("perm", PermissionPolicies.InstallationsClosureRequest));
            options.AddPolicy(PermissionPolicies.InstallationsClosureApprove, p => p.RequireClaim("perm", PermissionPolicies.InstallationsClosureApprove));

            options.AddPolicy(PermissionPolicies.InventoryPanelsRead, p => p.RequireClaim("perm", PermissionPolicies.InventoryPanelsRead));
            options.AddPolicy(PermissionPolicies.InventoryPanelsCreate, p => p.RequireClaim("perm", PermissionPolicies.InventoryPanelsCreate));
            options.AddPolicy(PermissionPolicies.InventoryPanelsUpdate, p => p.RequireClaim("perm", PermissionPolicies.InventoryPanelsUpdate));
            options.AddPolicy(PermissionPolicies.InventoryPanelsDelete, p => p.RequireClaim("perm", PermissionPolicies.InventoryPanelsDelete));
            options.AddPolicy(PermissionPolicies.InventoryInvertersRead, p => p.RequireClaim("perm", PermissionPolicies.InventoryInvertersRead));
            options.AddPolicy(PermissionPolicies.InventoryInvertersCreate, p => p.RequireClaim("perm", PermissionPolicies.InventoryInvertersCreate));
            options.AddPolicy(PermissionPolicies.InventoryMovementsRead, p => p.RequireClaim("perm", PermissionPolicies.InventoryMovementsRead));
            options.AddPolicy(PermissionPolicies.InventoryMovementsCreate, p => p.RequireClaim("perm", PermissionPolicies.InventoryMovementsCreate));

            options.AddPolicy(PermissionPolicies.AuditTrailRead, p => p
                .RequireRole("owner_admin")
                .RequireClaim("perm", PermissionPolicies.AuditTrailRead));

            options.AddPolicy(PermissionPolicies.AuthCapabilities, p => p.RequireClaim("perm", PermissionPolicies.AuthCapabilities));
        });

        return services;
    }
}
