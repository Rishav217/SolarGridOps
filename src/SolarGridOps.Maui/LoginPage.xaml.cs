using SolarGridOps.Maui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SolarGridOps.Maui;

public partial class LoginPage : ContentPage
{
    private readonly AuthApiClient _authApiClient;
    private readonly SessionState _sessionState;
    private CancellationTokenSource? _backgroundAnimationCts;

    public LoginPage()
    {
        InitializeComponent();
        _authApiClient = App.Services.GetRequiredService<AuthApiClient>();
        _sessionState = App.Services.GetRequiredService<SessionState>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _backgroundAnimationCts = new CancellationTokenSource();
        _ = RunBackgroundAnimationAsync(_backgroundAnimationCts.Token);
    }

    protected override void OnDisappearing()
    {
        _backgroundAnimationCts?.Cancel();
        _backgroundAnimationCts?.Dispose();
        _backgroundAnimationCts = null;
        base.OnDisappearing();
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

    private async Task RunBackgroundAnimationAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAll(
                    OrbOne.TranslateTo(-18, 14, 3200, Easing.CubicInOut),
                    OrbTwo.TranslateTo(14, -10, 3200, Easing.CubicInOut),
                    OrbOne.FadeTo(0.30, 3200, Easing.CubicInOut),
                    OrbTwo.FadeTo(0.24, 3200, Easing.CubicInOut));

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await Task.WhenAll(
                    OrbOne.TranslateTo(0, 0, 3200, Easing.CubicInOut),
                    OrbTwo.TranslateTo(0, 0, 3200, Easing.CubicInOut),
                    OrbOne.FadeTo(0.22, 3200, Easing.CubicInOut),
                    OrbTwo.FadeTo(0.17, 3200, Easing.CubicInOut));
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation to avoid noisy logs when page changes.
        }
    }
}
