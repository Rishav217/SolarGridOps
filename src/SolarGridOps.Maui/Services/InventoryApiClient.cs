using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SolarGridOps.Maui.Services;

public class InventoryApiClient
{
    private readonly HttpClient _httpClient;

    public InventoryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PanelInventoryItem>> GetPanelsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<ApiResponse<List<PanelInventoryItem>>>(
            $"api/v1/inventory/projects/{projectId}/panels",
            cancellationToken);

        if (payload?.Success != true || payload.Data is null)
        {
            return [];
        }

        return payload.Data;
    }

    public async Task<(bool IsSuccess, string? Error)> AddPanelAsync(Guid projectId, string serialNumber, int wattage, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/v1/inventory/projects/{projectId}/panels", new
        {
            serialNumber,
            wattage
        }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorPayload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
        return (false, errorPayload?.ErrorMessage ?? "Unable to create panel.");
    }

    public class PanelInventoryItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        [JsonPropertyName("wattage")]
        public int Wattage { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }
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
