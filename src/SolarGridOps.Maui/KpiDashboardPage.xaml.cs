namespace SolarGridOps.Maui;

public partial class KpiDashboardPage : ContentPage
{
    public KpiDashboardPage()
    {
        InitializeComponent();
    }

    private void OnRefreshSnapshotClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = $"Last synced: {DateTime.Now:HH:mm:ss}";
    }
}
