using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SolarGridOps.Maui.Services;

public class ProjectsApiClient
{
    private readonly HttpClient _httpClient;

    public ProjectsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProjectListItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<ApiResponse<List<ProjectListItem>>>(
            "api/v1/projects",
            cancellationToken);

        if (payload?.Success != true || payload.Data is null)
        {
            return [];
        }

        return payload.Data;
    }

    public async Task<ProjectListItem?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<ApiResponse<ProjectListItem>>(
            $"api/v1/projects/{projectId}",
            cancellationToken);

        return payload?.Success == true ? payload.Data : null;
    }

    public class ProjectListItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("customerId")]
        public Guid CustomerId { get; set; }

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("projectCode")]
        public string ProjectCode { get; set; } = string.Empty;

        [JsonPropertyName("capacityKW")]
        public decimal CapacityKW { get; set; }

        [JsonPropertyName("currentPhase")]
        public string CurrentPhase { get; set; } = string.Empty;

        [JsonPropertyName("siteAddress")]
        public string? SiteAddress { get; set; }
    }

    private class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
    }
}
