namespace SolarGridOps.Maui.Services;

public class SessionState
{
    private readonly List<string> _roles = [];

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
    }

    public void Clear()
    {
        AccessToken = null;
        UserFullName = null;
        _roles.Clear();
    }
}
