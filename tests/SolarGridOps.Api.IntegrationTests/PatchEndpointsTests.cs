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
public class PatchEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PatchEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProjectPhasePatch_RejectsBackwardPhase()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Agreement);

        var moveForward = await PatchJsonAsync($"/api/v1/projects/{projectId}/phase", new
        {
            currentPhase = ProjectPhase.Installation
        });

        Assert.Equal(HttpStatusCode.OK, moveForward.StatusCode);

        var moveBackward = await PatchJsonAsync($"/api/v1/projects/{projectId}/phase", new
        {
            currentPhase = ProjectPhase.Feasibility
        });

        Assert.Equal(HttpStatusCode.BadRequest, moveBackward.StatusCode);
    }

    [Fact]
    public async Task InstallationSessionPatch_RejectsEmptyRequestBody()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);
        var sessionId = await CreateSessionAsync(projectId);

        var response = await PatchJsonAsync($"/api/v1/installations/sessions/{sessionId}", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InstallationSessionPatch_UpdatesProvidedFields()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);
        var sessionId = await CreateSessionAsync(projectId);

        var response = await PatchJsonAsync($"/api/v1/installations/sessions/{sessionId}", new
        {
            workSummary = "Day 2 complete",
            isCompletedForDay = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);

        var data = doc.RootElement.GetProperty("data");
        Assert.Equal("Day 2 complete", data.GetProperty("workSummary").GetString());
        Assert.True(data.GetProperty("isCompletedForDay").GetBoolean());
    }

    [Fact]
    public async Task InstallationSession_RequestAndApproveClosure_UpdatesClosureStatus()
    {
        await AuthorizeAsync();
        var customerId = await CreateCustomerAsync();
        var projectId = await CreateProjectAsync(customerId, ProjectPhase.Installation);
        var sessionId = await CreateSessionAsync(projectId);

        var requestClosureResponse = await PostJsonAsync($"/api/v1/installations/sessions/{sessionId}/request-closure", new
        {
            customerSignatureName = "Durgesh",
            customerSignatureBase64 = "U0lHTkFUVVJF",
            notes = "All panels fitted"
        });
        Assert.Equal(HttpStatusCode.OK, requestClosureResponse.StatusCode);

        var approveClosureResponse = await PostJsonAsync($"/api/v1/installations/sessions/{sessionId}/approve-closure", new { });
        Assert.Equal(HttpStatusCode.OK, approveClosureResponse.StatusCode);

        var payload = await approveClosureResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(payload);
        var data = doc.RootElement.GetProperty("data");

        Assert.Equal(2, data.GetProperty("closureStatus").GetInt32());
        Assert.Equal("Durgesh", data.GetProperty("customerSignatureName").GetString());
    }

    [Fact]
    public async Task InstallationSessionPatch_MissingSession_WritesWarningLog()
    {
        await AuthorizeAsync();

        var response = await PatchJsonAsync($"/api/v1/installations/sessions/{Guid.NewGuid()}", new
        {
            workSummary = "Missing session"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logEntry = await db.ApplicationLogEntries.SingleAsync(x =>
            x.EventKey == "installations.session.session_missing" &&
            x.LogLevel == "Warning");

        Assert.Contains("updating session", logEntry.Message, StringComparison.OrdinalIgnoreCase);
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
            fullName = "Patch Test Customer",
            phoneNumber = $"95{suffix[^8..]}",
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
            projectCode = $"IT-PATCH-{suffix}",
            capacityKW = 5.0m,
            currentPhase = phase,
            installationStartDate = "2026-08-20T00:00:00Z",
            installationEndDate = "2026-08-25T00:00:00Z"
        });

        response.EnsureSuccessStatusCode();
        return await ExtractIdAsync(response);
    }

    private async Task<Guid> CreateSessionAsync(Guid projectId)
    {
        var response = await PostJsonAsync("/api/v1/installations/sessions", new
        {
            projectId,
            workSummary = "Initial session",
            isCompletedForDay = false
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
