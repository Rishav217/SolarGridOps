using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class MainPage : ContentPage
{
	private readonly InventoryApiClient _inventoryApiClient;

	public MainPage()
	{
		InitializeComponent();
		_inventoryApiClient = App.Services.GetRequiredService<InventoryApiClient>();
	}

	private async void OnRefreshClicked(object? sender, EventArgs e)
	{
		if (!Guid.TryParse(ProjectIdEntry.Text, out var projectId))
		{
			StatusLabel.Text = "Enter a valid project id.";
			return;
		}

		StatusLabel.Text = "Loading panels...";
		try
		{
			var panels = await _inventoryApiClient.GetPanelsAsync(projectId);
			PanelsCollectionView.ItemsSource = panels;
			StatusLabel.Text = $"Loaded {panels.Count} panel record(s).";
		}
		catch
		{
			StatusLabel.Text = "Unable to fetch panels. Verify API server and auth setup.";
		}
	}

	private async void OnCreatePanelClicked(object? sender, EventArgs e)
	{
		if (!Guid.TryParse(ProjectIdEntry.Text, out var projectId))
		{
			StatusLabel.Text = "Enter a valid project id.";
			return;
		}

		if (string.IsNullOrWhiteSpace(SerialEntry.Text) || !int.TryParse(WattageEntry.Text, out var wattage))
		{
			StatusLabel.Text = "Enter serial number and numeric wattage.";
			return;
		}

		StatusLabel.Text = "Creating panel...";
		var response = await _inventoryApiClient.AddPanelAsync(projectId, SerialEntry.Text.Trim(), wattage);
		if (!response.IsSuccess)
		{
			StatusLabel.Text = response.Error ?? "Panel create failed.";
			return;
		}

		SerialEntry.Text = string.Empty;
		WattageEntry.Text = string.Empty;
		StatusLabel.Text = "Panel created. Refreshing list...";
		OnRefreshClicked(sender, e);
	}
}
