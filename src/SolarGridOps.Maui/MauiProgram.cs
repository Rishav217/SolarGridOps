using Microsoft.Extensions.Logging;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var apiBaseAddress = DeviceInfo.Platform == DevicePlatform.Android
			? "http://10.0.2.2:5014/"
			: "http://localhost:5014/";

		builder.Services.AddSingleton(new HttpClient
		{
			BaseAddress = new Uri(apiBaseAddress)
		});
		builder.Services.AddSingleton<SessionState>();
		builder.Services.AddTransient<AuthApiClient>();
		builder.Services.AddTransient<InventoryApiClient>();
		builder.Services.AddTransient<InstallationApiClient>();
		builder.Services.AddTransient<AdminAuditApiClient>();
		builder.Services.AddTransient<ProjectsApiClient>();
		builder.Services.AddTransient<CustomersApiClient>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
