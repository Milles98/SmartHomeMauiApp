using Microsoft.Extensions.Logging;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.MVVM.Views;
using SmartHomeMauiApp.Resources.Converters;
using SmartHomeMauiApp.Services;
using System.Diagnostics;

namespace SmartHomeMauiApp;

public static class MauiProgram
{

    //Fake iothub: HostName=fake-iothub.azure-devices.net;SharedAccessKeyName=fakePolicy;SharedAccessKey=fakeSharedAccessKey12345

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

        builder.Services.AddSingleton<ISmarthomeContext, SmarthomeContext>();

        builder.Services.AddSingleton<IDeviceManager>(serviceProvider =>
        {
            var dbContext = serviceProvider.GetRequiredService<ISmarthomeContext>();
            var iotHubSettings = Task.Run(async () => await dbContext.GetIoTHubSettingsAsync()).Result;

            string connectionString = iotHubSettings?.ConnectionString ?? "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI=";
            return new DeviceManager(connectionString);
        });

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<SettingsPage>();

        builder.Services.AddSingleton<DeviceDetailViewModel>();
        builder.Services.AddSingleton<DeviceDetailPage>();

        builder.Services.AddSingleton<AddDeviceViewModel>();
        builder.Services.AddSingleton<AddDevicePage>();

        builder.Services.AddSingleton<HistoryViewModel>();
        builder.Services.AddSingleton<HistoryPage>();

        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
        builder.Services.AddSingleton<ITimerService, TimerService>();

        builder.Services.AddHttpClient<IWeatherService, WeatherService>();

        builder.Services.AddTransient<DeviceTypeToImageConverter>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        _ = InitializeDatabaseAsync(app.Services);

        return app;
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        try
        {
            var dbContext = services.GetRequiredService<ISmarthomeContext>();
            await dbContext.InitializeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error {ex}");
        }
    }
}
