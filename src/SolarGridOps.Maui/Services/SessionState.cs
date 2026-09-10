using System.Net.Http.Headers;

namespace SolarGridOps.Maui.Services;

public class SessionState
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _roles = [];

    public SessionState(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);
    public bool IsOwnerAdmin => _roles.Contains("owner_admin", StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<string> Roles => _roles;
    public string? AccessToken { get; private set; }
    public string? UserFullName { get; private set; }

    public void SetAuthenticated(string accessToken, string fullName, IEnumerable<string>? roles)
    {
        AccessToken = accessToken;
        UserFullName = fullName;
        _roles.Clear();
        if (roles is not null)
        {
            _roles.AddRange(roles.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
        }

        // Every API client shares this HttpClient, so this is the single place tokens get attached.
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public void Clear()
    {
        AccessToken = null;
        UserFullName = null;
        _roles.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}

