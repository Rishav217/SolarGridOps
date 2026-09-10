using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class MainPage : ContentPage
{
	private readonly SessionState _sessionState;
	private readonly AuthApiClient _authApiClient;
	public MainPage()
	{
		InitializeComponent();
		_sessionState = App.Services.GetRequiredService<SessionState>();
		_authApiClient = App.Services.GetRequiredService<AuthApiClient>();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		WelcomeLabel.Text = string.IsNullOrWhiteSpace(_sessionState.UserFullName)
			? "Welcome"
			: $"Welcome, {_sessionState.UserFullName}";
	}

	private async void OnWorkspaceClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//app/help");
	}

	private async void OnMetricCardTapped(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new ProjectsListPage());
	}

	private async void OnSignOutClicked(object? sender, EventArgs e)
	{
		await _authApiClient.LogoutAsync();
		_sessionState.Clear();
		await Shell.Current.GoToAsync("//login");
	}
}
