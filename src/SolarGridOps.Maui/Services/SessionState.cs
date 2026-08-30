namespace SolarGridOps.Maui.Services;

public class SessionState
{
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);
    public string? AccessToken { get; private set; }
    public string? UserFullName { get; private set; }

    public void SetAuthenticated(string accessToken, string fullName)
    {
        AccessToken = accessToken;
        UserFullName = fullName;
    }

    public void Clear()
    {
        AccessToken = null;
        UserFullName = null;
    }
}
