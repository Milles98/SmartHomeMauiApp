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

        builder.Services.AddSingleton<IDbContext, DbContext>();

        builder.Services.AddSingleton<IDeviceManager, DeviceManager>(serviceProvider =>
        {
            var dbContext = serviceProvider.GetRequiredService<IDbContext>();
            var iotHubSettings = dbContext.GetIoTHubSettingsAsync().Result;
            string connectionString = iotHubSettings?.ConnectionString ??
                                      "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI=";
            return new DeviceManager(connectionString);
        });

        if (!IsRunningUnitTests)
        {
            RegisterPlatformSpecificServices(builder.Services);
        }

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        SeedInitialData(app.Services);

        return app;
    }

    private static void RegisterPlatformSpecificServices(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainPage>();

        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<SettingsPage>();

        services.AddSingleton<DeviceDetailViewModel>();
        services.AddSingleton<DeviceDetailPage>();

        services.AddSingleton<AddDeviceViewModel>();
        services.AddSingleton<AddDevicePage>();

        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<HistoryPage>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddHttpClient<IWeatherService, WeatherService>();

        services.AddTransient<DeviceTypeToImageConverter>();
    }

    private static bool IsRunningUnitTests =>
        AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName!.StartsWith("xunit"));

    private static async void SeedInitialData(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<IDbContext>();

        await dbContext.SeedDataAsync(
            defaultEmail: "mille.elfver98@gmail.com",
            defaultConnectionString: "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI="
        );
    }
}
