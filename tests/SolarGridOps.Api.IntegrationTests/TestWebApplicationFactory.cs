using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SolarGridOps.Api.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"SolarGridOps_Test_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Server=localhost;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
                ["Jwt:Issuer"] = "SolarGridOps",
                ["Jwt:Audience"] = "SolarGridOpsClients",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Jwt:Secret"] = "DEV_ONLY_CHANGE_BEFORE_PRODUCTION_MIN32CHARS!"
            };

            config.AddInMemoryCollection(overrides);
        });
    }
}
