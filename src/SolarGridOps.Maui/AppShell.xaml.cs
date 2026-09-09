using Microsoft.Extensions.DependencyInjection;

namespace SolarGridOps.Maui;

public partial class AppShell : Shell
{
	private readonly Services.SessionState _sessionState;

	public AppShell()
	{
		InitializeComponent();
		_sessionState = App.Services.GetRequiredService<Services.SessionState>();
		RefreshRoleBasedTabs();
		_ = GoToAsync("//login");
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);
		RefreshRoleBasedTabs();
	}

	private void RefreshRoleBasedTabs()
	{
		AdminLogsTab.IsVisible = _sessionState.IsAuthenticated && _sessionState.IsOwnerAdmin;
	}
}
