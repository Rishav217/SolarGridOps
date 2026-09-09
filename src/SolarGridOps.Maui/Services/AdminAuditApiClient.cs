using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SolarGridOps.Maui.Services;

public class AdminAuditApiClient
{
    private readonly HttpClient _httpClient;

    public AdminAuditApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AdminAuditLogItem>> ListRecentAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<ApiResponse<List<AdminAuditLogItem>>>(
            $"api/v1/admin/audit-trail?take={Math.Clamp(take, 1, 250)}",
            cancellationToken);

        if (payload?.Success != true || payload.Data is null)
        {
            return [];
        }

        return payload.Data;
    }

    public class AdminAuditLogItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; }

        [JsonPropertyName("actionKey")]
        public string ActionKey { get; set; } = string.Empty;

        [JsonPropertyName("entityType")]
        public string EntityType { get; set; } = string.Empty;

        [JsonPropertyName("entityId")]
        public Guid? EntityId { get; set; }

        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [JsonPropertyName("actorName")]
        public string ActorName { get; set; } = "System";
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
