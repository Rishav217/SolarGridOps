using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SolarGridOps.Api.Models;
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
        if (jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");
        }
        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
        {
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");
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

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var payload = ApiResponse<object>.Fail("UNAUTHORIZED", "Authentication is required to access this resource.");
                        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        await context.Response.WriteAsync(json);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var payload = ApiResponse<object>.Fail("FORBIDDEN", "You do not have permission to perform this action.");
                        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        await context.Response.WriteAsync(json);
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.FallbackPolicy = options.DefaultPolicy;

            options.AddPolicy(PermissionPolicies.CustomersRead, policy =>
                policy.RequireAuthenticatedUser().RequireClaim("perm", PermissionPolicies.CustomersRead));

            options.AddPolicy(PermissionPolicies.CustomersCreate, policy =>
                policy.RequireAuthenticatedUser().RequireClaim("perm", PermissionPolicies.CustomersCreate));

            options.AddPolicy(PermissionPolicies.AuthCapabilities, policy =>
                policy.RequireAuthenticatedUser().RequireClaim("perm", PermissionPolicies.AuthCapabilities));

            options.AddPolicy(PermissionPolicies.AuthLogout, policy =>
                policy.RequireAuthenticatedUser().RequireClaim("perm", PermissionPolicies.AuthLogout));
        });

        return services;
    }
}
