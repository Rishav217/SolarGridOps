namespace SolarGridOps.Maui;

public partial class KpiDashboardPage : ContentPage
{
    private const string LowStockAlertMessage = "Attention: low panel stock detected. Review highlighted items below and take action.";

    public KpiDashboardPage()
    {
        InitializeComponent();
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
