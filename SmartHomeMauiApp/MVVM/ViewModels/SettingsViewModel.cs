using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Services;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;

    [ObservableProperty]
    private string _connectionString;

    [ObservableProperty]
    private string _emailAddress;

    [ObservableProperty]
    private string _responseMessage;

    public SettingsViewModel(DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
        ResponseMessage = string.Empty;
        ConnectionString = "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI="; // Sätt en standard connection string om det behövs
        EmailAddress = "mille.elfver98@gmail.com";
    }

    [RelayCommand]
    private async Task NavigateHomeAsync()
    {
        await Shell.Current.GoToAsync("//MainPage");
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
