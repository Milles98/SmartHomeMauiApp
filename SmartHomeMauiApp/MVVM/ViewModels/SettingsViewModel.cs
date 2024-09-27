using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;
    private readonly DbContext _dbContext;

    [ObservableProperty]
    private string? _connectionString;

    [ObservableProperty]
    private string? _emailAddress;

    [ObservableProperty]
    private string? _responseMessage;

    public SettingsViewModel(DeviceManager deviceManager, DbContext dbContext)
    {
        _deviceManager = deviceManager;
        _dbContext = dbContext;
        ResponseMessage = string.Empty;

        LoadSettings();
    }

    private async void LoadSettings()
    {
        var userSettings = await _dbContext.GetUserSettingsAsync();
        EmailAddress = userSettings?.EmailAddress ?? string.Empty;

        var iotHubSettings = await _dbContext.GetIoTHubSettingsAsync();
        ConnectionString = iotHubSettings?.ConnectionString ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                ResponseMessage = "Connection String cannot be empty.";
                return;
            }

            _deviceManager.UpdateConnectionString(ConnectionString);

            var userSettings = new UserSettings { EmailAddress = EmailAddress };
            await _dbContext.SaveUserSettingsAsync(userSettings);

            var iotHubSettings = new IoTHubSettings { ConnectionString = ConnectionString };
            await _dbContext.SaveIoTHubSettingsAsync(iotHubSettings);

            ResponseMessage = "Settings have been saved, and IoT Hub connection has been updated.";
            Preferences.Set("EmailAddress", EmailAddress);
        }
        catch (Exception ex)
        {
            ResponseMessage = "An error occurred while saving settings. Please try again.";
            Debug.WriteLine($"Error in SaveSettingsAsync: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
