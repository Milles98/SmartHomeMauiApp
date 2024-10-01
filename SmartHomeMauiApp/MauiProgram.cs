using Microsoft.Extensions.Logging;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.MVVM.Views;
using SmartHomeMauiApp.Resources.Converters;

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

        builder.Services.AddTransient<DeviceTypeToImageConverter>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        SeedInitialData(app.Services);

        return app;
    }

    private static async void SeedInitialData(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<IDbContext>();

        await dbContext.SeedDataAsync(
            defaultEmail: "mille.elfver98@gmail.com",
            defaultConnectionString: "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI="
        );
    }
}
