using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Api.IntegrationTests;

[Collection("ApiIntegration")]
public class InverterEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public InverterEndpointsTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddInverter_AndList_ReturnsCreatedInverter()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var createResponse = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/inverters", new
        {
            serialNumber = serial,
            capacityKva = 5.0m,
            brand = "VoltPro",
            model = "VP-5",
            notes = "Initial inverter"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/v1/inventory/projects/{projectId}/inverters");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var payload = await listResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        var inverters = doc.RootElement.GetProperty("data");

        Assert.True(inverters.GetArrayLength() >= 1);
        var containsCreated = inverters.EnumerateArray().Any(x => x.GetProperty("serialNumber").GetString() == serial);
        Assert.True(containsCreated);
    }

    [Fact]
    public async Task AddInverter_WithDuplicateSerial_ReturnsConflict()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"INV-DUP-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var first = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/inverters", new
        {
            serialNumber = serial,
            capacityKva = 3.5m
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/inverters", new
        {
            serialNumber = serial,
            capacityKva = 3.5m
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task InverterEndpoints_RequireAuthentication()
    {
        var response = await _client.GetAsync($"/api/v1/inventory/projects/{Guid.NewGuid()}/inverters");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task AuthorizeAsync()
    {
        var loginResponse = await PostJsonAsync("/api/v1/auth/login", new
        {
            usernameOrMobile = "9999999999",
            password = "Admin@123"
        });

        loginResponse.EnsureSuccessStatusCode();

        var payload = await loginResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        var accessToken = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private async Task<Guid> CreateCustomerAsync()
    {
        var suffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var response = await PostJsonAsync("/api/v1/customers", new
        {
            fullName = "Inverter Test Customer",
            phoneNumber = $"97{suffix[^8..]}",
            address = "Pune",
            city = "Pune",
            state = "MH"
        });

        response.EnsureSuccessStatusCode();
        return await ExtractIdAsync(response);
    }

    private async Task<Guid> CreateProjectAsync(Guid customerId, ProjectPhase phase)
    {
        var suffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var response = await PostJsonAsync("/api/v1/projects", new
        {
            customerId,
            projectCode = $"IT-INV-{suffix}",
            capacityKW = 7.0m,
            currentPhase = phase,
            installationStartDate = "2026-08-20T00:00:00Z",
            installationEndDate = "2026-08-25T00:00:00Z"
        });

        response.EnsureSuccessStatusCode();
        return await ExtractIdAsync(response);
    }

    private async Task<Guid> ExtractIdAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    private Task<HttpResponseMessage> PostJsonAsync(string url, object body)
    {
        var content = JsonSerializer.Serialize(body, JsonOptions);
        return _client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
    }
}
