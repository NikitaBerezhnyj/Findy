using Findy.Pages;
using Findy.Services;
using Findy.ViewModels;
using CommunityToolkit.Maui;
using MauiIcons.Material;
using Microsoft.Extensions.Logging;

namespace Findy;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMaterialMauiIcons()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<BluetoothService>();

		builder.Services.AddTransient<DevicesViewModel>();
		builder.Services.AddTransient<DevicesPage>();

		builder.Services.AddTransient<RadarViewModel>();
		builder.Services.AddTransient<RadarPage>();

		return builder.Build();
	}
}