using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class MainPage : ContentPage
{
	private readonly InstallationApiClient _installationApiClient;

	public MainPage()
	{
		InitializeComponent();
		_installationApiClient = App.Services.GetRequiredService<InstallationApiClient>();
	}

	private async void OnRefreshClicked(object? sender, EventArgs e)
	{
		if (!Guid.TryParse(ProjectIdEntry.Text, out var projectId))
		{
			StatusLabel.Text = "Enter a valid project id.";
			return;
		}

		StatusLabel.Text = "Loading sessions...";
		try
		{
			var sessions = await _installationApiClient.GetSessionsAsync(projectId);
			SessionsCollectionView.ItemsSource = sessions;
			StatusLabel.Text = $"Loaded {sessions.Count} session record(s).";
		}
		catch
		{
			StatusLabel.Text = "Unable to fetch sessions. Verify API server and auth setup.";
		}
	}

	private async void OnCreateSessionClicked(object? sender, EventArgs e)
	{
		if (!Guid.TryParse(ProjectIdEntry.Text, out var projectId))
		{
			StatusLabel.Text = "Enter a valid project id.";
			return;
		}

		if (string.IsNullOrWhiteSpace(WorkSummaryEditor.Text))
		{
			StatusLabel.Text = "Work summary is required.";
			return;
		}

		StatusLabel.Text = "Creating session...";
		var result = await _installationApiClient.CreateSessionAsync(projectId, WorkSummaryEditor.Text.Trim(), CompletedForDayCheckBox.IsChecked);
		if (!result.IsSuccess)
		{
			StatusLabel.Text = result.Error ?? "Session create failed.";
			return;
		}

		WorkSummaryEditor.Text = string.Empty;
		CompletedForDayCheckBox.IsChecked = false;
		StatusLabel.Text = "Session created. Refreshing list...";
		OnRefreshClicked(sender, e);
	}
}
