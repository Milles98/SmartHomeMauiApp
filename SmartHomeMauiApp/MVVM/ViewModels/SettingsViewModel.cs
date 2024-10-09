using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.Services;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDeviceManager _deviceManager;
    private readonly ISmarthomeContext _dbContext;
    private readonly IPreferencesService _preferencesService;

    [ObservableProperty]
    private string? _connectionString;

    [ObservableProperty]
    private string? _emailAddress;

    [ObservableProperty]
    private string? _responseMessage;

    [ObservableProperty]
    private string _responseMessageColor = "Red";

    public SettingsViewModel(IDeviceManager deviceManager, ISmarthomeContext dbContext, IPreferencesService preferencesService)
    {
        _deviceManager = deviceManager;
        _dbContext = dbContext;
        _preferencesService = preferencesService;
        ResponseMessage = string.Empty;
    }

    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();
    }

    public async Task LoadSettingsAsync()
    {
        var userSettings = await _dbContext.GetUserSettingsAsync();
        EmailAddress = userSettings?.EmailAddress ?? string.Empty;

        var iotHubSettings = await _dbContext.GetIoTHubSettingsAsync();
        ConnectionString = iotHubSettings?.ConnectionString ?? "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI=";
    }

    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                ResponseMessage = "Connection String cannot be empty.";
                return;
            }

            _deviceManager.UpdateConnectionString(ConnectionString);

            Debug.WriteLine("Saving settings...");

            var userSettings = new UserSettings { EmailAddress = EmailAddress };
            await _dbContext.SaveUserSettingsAsync(userSettings);

            var iotHubSettings = new IoTHubSettings { ConnectionString = ConnectionString };
            await _dbContext.SaveIoTHubSettingsAsync(iotHubSettings);

            ResponseMessage = "Settings have been saved, and IoT Hub connection has been updated.";
            ResponseMessageColor = "Green";
            _preferencesService.Set("EmailAddress", EmailAddress!);
        }
        catch (Exception ex)
        {
            ResponseMessage = "An error occurred while saving settings. Please try again.";
            ResponseMessageColor = "Red";
            Debug.WriteLine($"Error in SaveSettingsAsync: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
