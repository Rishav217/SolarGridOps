using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SolarGridOps.Maui.Services;

public class InstallationApiClient
{
    private readonly HttpClient _httpClient;

    public InstallationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<InstallationSessionItem>> GetSessionsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<ApiResponse<List<InstallationSessionItem>>>(
            $"api/v1/installations/projects/{projectId}/sessions",
            cancellationToken);

        if (payload?.Success != true || payload.Data is null)
        {
            return [];
        }

        return payload.Data;
    }

    public async Task<(bool IsSuccess, string? Error)> CreateSessionAsync(Guid projectId, string summary, bool completedForDay, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/installations/sessions", new
        {
            projectId,
            workSummary = summary,
            isCompletedForDay = completedForDay
        }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorPayload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
        return (false, errorPayload?.ErrorMessage ?? "Unable to create session.");
    }

    public class InstallationSessionItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("sessionDateUtc")]
        public DateTime SessionDateUtc { get; set; }

        [JsonPropertyName("workSummary")]
        public string? WorkSummary { get; set; }

        [JsonPropertyName("isCompletedForDay")]
        public bool IsCompletedForDay { get; set; }
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
