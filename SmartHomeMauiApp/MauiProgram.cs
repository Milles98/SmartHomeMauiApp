using Microsoft.Extensions.Logging;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.MVVM.Views;
using SmartHomeMauiApp.Resources.Converters;
using SmartHomeMauiApp.Services;

namespace SmartHomeMauiApp;

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

        builder.Services.AddSingleton<ISmarthomeContext, SmarthomeContext>();

        builder.Services.AddSingleton<IDeviceManager, DeviceManager>(serviceProvider =>
        {
            string connectionString = "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI=";
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
        _ = SeedInitialData(app.Services);

        return app;
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ISmarthomeContext>();
        await dbContext.InitializeAsync();
    }

    private static async Task SeedInitialData(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ISmarthomeContext>();

        await dbContext.SeedDataIntoDbAsync(
            defaultConnectionString: "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI="
        );
    }
}
