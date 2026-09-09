namespace SolarGridOps.Maui;

using Microsoft.Maui.Graphics;
using SolarGridOps.Maui.Controls;

public partial class KpiDashboardPage : ContentPage
{
    private const string LowStockAlertMessage = "Attention: low panel stock detected. Review highlighted items below and take action.";

    public KpiDashboardPage()
    {
        InitializeComponent();
        StockTrendGraphicsView.Drawable = new LineSparklineDrawable
        {
            Values = [310f, 340f, 322f, 360f, 355f, 390f, 426f],
            LineColor = Color.FromArgb("#2F6FED"),
            FillColor = Color.FromArgb("#332F6FED")
        };

        CategoryDonutGraphicsView.Drawable = new DonutChartDrawable
        {
            Segments =
            [
                new DonutSegment { Value = 426, Color = Color.FromArgb("#2F6FED") },
                new DonutSegment { Value = 96, Color = Color.FromArgb("#1FA463") },
                new DonutSegment { Value = 40, Color = Color.FromArgb("#F5A623") }
            ]
        };
    }

    private async void OnOpenStockAlertsClicked(object? sender, EventArgs e)
    {
        var encodedAlert = Uri.EscapeDataString(LowStockAlertMessage);
        await Shell.Current.GoToAsync($"//app/inventory?alert={encodedAlert}");
    }

    private async void OnOpenInstallationsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//app/installations");
    }

    private void OnRefreshSnapshotClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = $"Last synced: {DateTime.Now:HH:mm:ss}";
    }
}
