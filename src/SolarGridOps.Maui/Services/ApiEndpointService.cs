namespace SolarGridOps.Maui.Services;

public static class ApiEndpointService
{
    private const string PreferenceKey = "ApiBaseAddress";

    public static string GetBaseAddress()
    {
        var saved = Preferences.Default.Get(PreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(saved))
        {
            return saved;
        }

        // Emulator localhost maps through 10.0.2.2; a real device needs the PC's LAN IP set from the login screen.
        return DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5014/"
            : "http://localhost:5014/";
    }

    public static void SetBaseAddress(string baseAddress)
    {
        var normalized = baseAddress.Trim();
        if (!normalized.EndsWith('/'))
        {
            normalized += "/";
        }

        Preferences.Default.Set(PreferenceKey, normalized);
    }
}
