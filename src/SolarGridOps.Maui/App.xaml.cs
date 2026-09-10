using Microsoft.Extensions.DependencyInjection;

namespace SolarGridOps.Maui;

public partial class App : Application
{
	public static IServiceProvider Services { get; private set; } = null!;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		Services = services;

		// App is only designed for light colors; force it so OS dark mode never makes text invisible.
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}