using Microsoft.Extensions.Logging;
using Shared.Library.Services;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.MVVM.Views;

namespace SmartHomeMauiApp
{
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
					fonts.AddFont("fa-brands-400.ttf", "fa-brands");
					fonts.AddFont("fa-solid-900.ttf", "fa-solid");
				});

			builder.Services.AddSingleton<MainViewModel>();
			builder.Services.AddSingleton<MainPage>();
			builder.Services.AddSingleton<DeviceManager>();

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
