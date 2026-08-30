using SolarGridOps.Maui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SolarGridOps.Maui;

public partial class LoginPage : ContentPage
{
    private readonly AuthApiClient _authApiClient;
    private readonly SessionState _sessionState;

    public LoginPage()
    {
        InitializeComponent();
        _authApiClient = App.Services.GetRequiredService<AuthApiClient>();
        _sessionState = App.Services.GetRequiredService<SessionState>();
    }

    private void OnShowPasswordCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        PasswordEntry.IsPassword = !e.Value;
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            StatusLabel.Text = "Enter username/mobile and password.";
            return;
        }

        SignInButton.IsEnabled = false;
        StatusLabel.Text = "Signing in...";

        try
        {
            var result = await _authApiClient.LoginAsync(UsernameEntry.Text.Trim(), PasswordEntry.Text);
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.FullName))
            {
                StatusLabel.Text = result.ErrorMessage ?? "Invalid credentials.";
                return;
            }

            _sessionState.SetAuthenticated(result.AccessToken, result.FullName);
            await Shell.Current.GoToAsync("//app/dashboard");
        }
        catch (Exception)
        {
            StatusLabel.Text = "Unable to reach server. Verify API URL and network.";
        }
        finally
        {
            SignInButton.IsEnabled = true;
        }
    }
}
