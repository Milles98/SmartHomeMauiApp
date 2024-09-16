using Microsoft.Extensions.Logging;
using Shared.Library.Services;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.MVVM.Views;
using SmartHomeMauiApp.Resources.Converters;

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

            builder.Services.AddSingleton(new DeviceManager("HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI="));
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<SettingsPage>();

            builder.Services.AddSingleton<DeviceDetailViewModel>();
            builder.Services.AddSingleton<DeviceDetailPage>();

            builder.Services.AddSingleton<AddDeviceViewModel>();
            builder.Services.AddSingleton<AddDevicePage>();

            builder.Services.AddTransient<DeviceTypeToImageConverter>();
            builder.Services.AddSingleton<ConnectionStateToColorConverter>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
