namespace SolarGridOps.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		_ = GoToAsync("//login");
	}
}
