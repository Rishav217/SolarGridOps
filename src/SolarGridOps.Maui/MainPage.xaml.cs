namespace SolarGridOps.Maui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnRefreshSnapshotClicked(object? sender, EventArgs e)
	{
		StatusLabel.Text = $"Last synced: {DateTime.Now:HH:mm:ss}";
	}
}
