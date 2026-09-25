namespace SolarGridOps.Maui;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using SolarGridOps.Maui.Controls;
using SolarGridOps.Maui.Services;

public partial class KpiDashboardPage : ContentPage
{
    private readonly DashboardApiClient _dashboardApiClient;

    public KpiDashboardPage()
    {
        InitializeComponent();
        _dashboardApiClient = App.Services.GetRequiredService<DashboardApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            StatusLabel.Text = "Loading live data...";
            var summary = await _dashboardApiClient.GetSummaryAsync();
            if (summary is null)
            {
                StatusLabel.Text = "Unable to load live data.";
                return;
            }

            ActiveProjectsValueLabel.Text = summary.ActiveProjectsCount.ToString();
            InstallationSessionsValueLabel.Text = summary.InstallationSessionsCount.ToString();
            PanelsAssignedValueLabel.Text = summary.TotalPanelsCount.ToString();
            PendingClosuresValueLabel.Text = summary.PendingClosureSessionsCount.ToString();

            CategoryDonutGraphicsView.Drawable = new DonutChartDrawable
            {
                Segments =
                [
                    new DonutSegment { Value = Math.Max(summary.TotalPanelsCount, 0.001f), Color = Color.FromArgb("#2F6FED") },
                    new DonutSegment { Value = Math.Max(summary.TotalInvertersCount, 0.001f), Color = Color.FromArgb("#1FA463") },
                    new DonutSegment { Value = Math.Max(summary.PendingClosureSessionsCount, 0.001f), Color = Color.FromArgb("#E5484D") }
                ]
            };
            CategoryDonutGraphicsView.Invalidate();
            DonutLegendLabel.Text = $"Panels {summary.TotalPanelsCount} | Inverters {summary.TotalInvertersCount} | Pending {summary.PendingClosureSessionsCount}";

            PendingApprovalsMessageLabel.Text = summary.PendingClosureSessionsCount > 0
                ? $"{summary.PendingClosureSessionsCount} installation session(s) are waiting for closure approval."
                : "No installation sessions are currently waiting for approval.";

            StatusLabel.Text = $"Last synced: {DateTime.Now:HH:mm:ss}";
        }
        catch
        {
            StatusLabel.Text = "Unable to load live data. Check server connection.";
        }
    }

    private async void OnOpenInstallationsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//app/installations");
    }

    private async void OnRefreshSnapshotClicked(object? sender, EventArgs e)
    {
        await LoadAsync();
    }
}

