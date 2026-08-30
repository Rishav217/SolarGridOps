using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Infrastructure.Persistence;
using SolarGridOps.Domain.Enums;

namespace SolarGridOps.Api.IntegrationTests;

[Collection("ApiIntegration")]
public class InventoryEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public InventoryEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddPanel_AndList_ReturnsCreatedPanel()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"IT-PANEL-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var createResponse = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber = serial,
            wattage = 550,
            brand = "SunPrime",
            model = "SP-550",
            notes = "Batch A"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/v1/inventory/projects/{projectId}/panels");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var payload = await listResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        var panels = doc.RootElement.GetProperty("data");

        Assert.True(panels.GetArrayLength() >= 1);
        var containsCreated = panels.EnumerateArray().Any(x => x.GetProperty("serialNumber").GetString() == serial);
        Assert.True(containsCreated);
    }

    [Fact]
    public async Task AddPanel_WritesAuditTrailEntry()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"IT-AUD-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var createResponse = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber = serial,
            wattage = 545
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var auditEntry = await db.AuditTrailEntries.SingleAsync(x =>
            x.ActionKey == "inventory.panel.created" &&
            x.EntityType == "panel" &&
            x.Details != null &&
            x.Details.Contains(serial));

        Assert.Contains(serial, auditEntry.Details);
    }

    [Fact]
    public async Task AddPanel_WithDuplicateSerial_ReturnsConflict()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"IT-DUP-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var first = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber = serial,
            wattage = 540
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber = serial,
            wattage = 540
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task UpdatePanel_ChangesProvidedFields()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"IT-UPD-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var createResponse = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber = serial,
            wattage = 530,
            brand = "OldBrand"
        });
        var panelId = await ExtractIdAsync(createResponse);

        var updateResponse = await PatchJsonAsync($"/api/v1/inventory/panels/{panelId}", new
        {
            wattage = 575,
            brand = "NewBrand",
            notes = "Upgraded"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var payload = await updateResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        var panel = doc.RootElement.GetProperty("data");

        Assert.Equal(575, panel.GetProperty("wattage").GetInt32());
        Assert.Equal("NewBrand", panel.GetProperty("brand").GetString());
        Assert.Equal("Upgraded", panel.GetProperty("notes").GetString());
    }

    [Fact]
    public async Task RemovePanel_MarksPanelDeleted_AndListDoesNotReturnIt()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);

        var serial = $"IT-DEL-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var createResponse = await PostJsonAsync($"/api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber = serial,
            wattage = 540
        });
        var panelId = await ExtractIdAsync(createResponse);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/inventory/panels/{panelId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/v1/inventory/projects/{projectId}/panels");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var payload = await listResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        var panels = doc.RootElement.GetProperty("data");

        var containsDeleted = panels.EnumerateArray().Any(x => x.GetProperty("serialNumber").GetString() == serial);
        Assert.False(containsDeleted);
    }

    [Fact]
    public async Task InventoryEndpoints_RequireAuthentication()
    {
        var response = await _client.GetAsync($"/api/v1/inventory/projects/{Guid.NewGuid()}/panels");
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
            fullName = "Inventory Test Customer",
            phoneNumber = $"96{suffix[^8..]}",
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
            capacityKW = 6.0m,
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

    private Task<HttpResponseMessage> PatchJsonAsync(string url, object body)
    {
        var content = JsonSerializer.Serialize(body, JsonOptions);
        return _client.PatchAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
    }
}
