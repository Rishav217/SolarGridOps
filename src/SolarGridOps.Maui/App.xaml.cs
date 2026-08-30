using Microsoft.Extensions.DependencyInjection;

namespace SolarGridOps.Maui;

public partial class App : Application
{
	public static IServiceProvider Services { get; private set; } = null!;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		Services = services;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}