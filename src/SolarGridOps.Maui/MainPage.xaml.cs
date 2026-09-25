using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class MainPage : ContentPage
{
	private readonly SessionState _sessionState;
	private readonly AuthApiClient _authApiClient;
	private readonly DashboardApiClient _dashboardApiClient;
	public MainPage()
	{
		InitializeComponent();
		_sessionState = App.Services.GetRequiredService<SessionState>();
		_authApiClient = App.Services.GetRequiredService<AuthApiClient>();
		_dashboardApiClient = App.Services.GetRequiredService<DashboardApiClient>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		WelcomeLabel.Text = string.IsNullOrWhiteSpace(_sessionState.UserFullName)
			? "Welcome"
			: $"Welcome, {_sessionState.UserFullName}";

		try
		{
			var summary = await _dashboardApiClient.GetSummaryAsync();
			if (summary is null)
			{
				LiveStatusLabel.Text = "Unable to load live data.";
				return;
			}

			ActiveProjectsValueLabel.Text = summary.ActiveProjectsCount.ToString();
			InstallSessionsValueLabel.Text = summary.InstallationSessionsCount.ToString();
			PanelsAssignedValueLabel.Text = summary.TotalPanelsCount.ToString();
			PendingClosuresValueLabel.Text = summary.PendingClosureSessionsCount.ToString();
			LiveStatusLabel.Text = $"{summary.TotalProjectsCount} total project(s) tracked. Last synced {DateTime.Now:HH:mm:ss}.";
		}
		catch
		{
			LiveStatusLabel.Text = "Unable to load live data. Check server connection.";
		}
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
