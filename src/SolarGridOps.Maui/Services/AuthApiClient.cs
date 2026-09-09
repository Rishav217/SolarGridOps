using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SolarGridOps.Maui.Services;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage, string? FullName, string? AccessToken, IReadOnlyList<string> Roles)> LoginAsync(string usernameOrMobile, string password, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", new LoginPayload
        {
            UsernameOrMobile = usernameOrMobile,
            Password = password
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var failed = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: cancellationToken);
            return (false, failed?.ErrorMessage ?? "Login failed.", null, null, []);
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: cancellationToken);
        if (payload?.Success != true || payload.Data is null)
        {
            return (false, payload?.ErrorMessage ?? "Invalid login response.", null, null, []);
        }

        return (true, null, payload.Data.FullName, payload.Data.AccessToken, payload.Data.Roles ?? []);
    }

    private sealed class LoginPayload
    {
        [JsonPropertyName("usernameOrMobile")]
        public string UsernameOrMobile { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    private sealed class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
    }

    private sealed class AuthResponse
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }
    }
}
