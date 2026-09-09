using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

[QueryProperty(nameof(AlertMessage), "alert")]
[QueryProperty(nameof(IncomingProjectId), "projectId")]
public partial class InventoryWorkflowPage : ContentPage
{
    private readonly InventoryApiClient _inventoryApiClient;
    private string? _alertMessage;

    public string? AlertMessage
    {
        get => _alertMessage;
        set
        {
            _alertMessage = value;
            UpdateAlertBanner();
        }
    }

    public string? IncomingProjectId
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            ProjectIdEntry.Text = Uri.UnescapeDataString(value);
            OnRefreshClicked(this, EventArgs.Empty);
        }
    }

    public InventoryWorkflowPage()
    {
        InitializeComponent();
        _inventoryApiClient = App.Services.GetRequiredService<InventoryApiClient>();
        UpdateAlertBanner();
    }

    private void UpdateAlertBanner()
    {
        if (AlertBanner is null || AlertBannerLabel is null)
        {
            return;
        }

        var message = string.IsNullOrWhiteSpace(_alertMessage) ? null : Uri.UnescapeDataString(_alertMessage);
        AlertBanner.IsVisible = !string.IsNullOrWhiteSpace(message);
        AlertBannerLabel.Text = message ?? string.Empty;
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
