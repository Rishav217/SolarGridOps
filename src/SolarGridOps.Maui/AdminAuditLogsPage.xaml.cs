using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class AdminAuditLogsPage : ContentPage
{
    private readonly AdminAuditApiClient _adminAuditApiClient;

    public AdminAuditLogsPage()
    {
        InitializeComponent();
        _adminAuditApiClient = App.Services.GetRequiredService<AdminAuditApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            StatusLabel.Text = "Loading recent activity...";
            var entries = await _adminAuditApiClient.ListRecentAsync(120);
            LogsCollectionView.ItemsSource = entries
                .Select(x => new AuditLogRow
                {
                    ActionKey = x.ActionKey,
                    Details = string.IsNullOrWhiteSpace(x.Details) ? "No extra details." : x.Details,
                    MetaLine = $"{x.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | {x.ActorName} | {x.EntityType}"
                })
                .ToList();
            StatusLabel.Text = $"Loaded {entries.Count} log entries.";
        }
        catch
        {
            StatusLabel.Text = "Unable to load logs. Only admin users can access this data.";
        }
    }

    private sealed class AuditLogRow
    {
        public string ActionKey { get; set; } = string.Empty;
        public string MetaLine { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
