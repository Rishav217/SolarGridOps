using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SolarGridOps.Maui.Services;

public class DashboardApiClient
{
    private readonly HttpClient _httpClient;

    public DashboardApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardSummaryItem?> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardSummaryItem>>(
            "api/v1/dashboard/summary",
            cancellationToken);

        return payload?.Success == true ? payload.Data : null;
    }

    public class DashboardSummaryItem
    {
        [JsonPropertyName("totalProjectsCount")]
        public int TotalProjectsCount { get; set; }

        [JsonPropertyName("activeProjectsCount")]
        public int ActiveProjectsCount { get; set; }

        [JsonPropertyName("installationSessionsCount")]
        public int InstallationSessionsCount { get; set; }

        [JsonPropertyName("pendingClosureSessionsCount")]
        public int PendingClosureSessionsCount { get; set; }

        [JsonPropertyName("totalPanelsCount")]
        public int TotalPanelsCount { get; set; }

        [JsonPropertyName("totalInvertersCount")]
        public int TotalInvertersCount { get; set; }
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
