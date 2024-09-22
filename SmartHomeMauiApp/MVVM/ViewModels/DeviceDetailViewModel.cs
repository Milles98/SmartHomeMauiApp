using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.Views;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

[QueryProperty(nameof(DeviceId), "deviceId")]
[QueryProperty(nameof(EmailAddress), "emailAddress")]
public partial class DeviceDetailViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;
    private readonly MainViewModel _mainViewModel;
    private readonly DbContext _dbContext;
    private bool _isRemovingDevice;

    [ObservableProperty]
    private string _deviceId;

    [ObservableProperty]
    private string _status;

    [ObservableProperty]
    private string _connectionState;

    [ObservableProperty]
    private string _lastActivityTime;

    [ObservableProperty]
    private string _deviceState;

    [ObservableProperty]
    private string _deviceType;

    [ObservableProperty]
    private string _deviceName;

    [ObservableProperty]
    private string _emailAddress;

    [ObservableProperty]
    private string _responseMessage;

    [ObservableProperty]
    private string _responseMessageColor = "Red";

    private System.Timers.Timer _updateTimer;

    public bool IsRemoveDeviceVisible =>
        DeviceId != "new-fan-bd437070-7751-45ca-8040-d484cedd6201" &&
        DeviceId != "ac-3cea3c99-c45a-4f44-a8ea-1fb70b9d2dca" &&
        DeviceId != "new-lamp-33c0d9c6-66f2-4aa6-bef5-c3d4417bc74c";

    public DeviceDetailViewModel(DeviceManager deviceManager, MainViewModel mainViewModel, DbContext dbContext)
    {
        _deviceManager = deviceManager;
        _mainViewModel = mainViewModel;
        _dbContext = dbContext;

        ResponseMessage = string.Empty;

        LoadUserSettings().ConfigureAwait(false);

        //FIXA VID BORTTAGNING OM EN ENHET INTE LÄNGRE EXISTERAR, BUG
        _updateTimer = new System.Timers.Timer(5000);
        _updateTimer.Elapsed += async (sender, e) => await LoadDeviceDetailsAsync(DeviceId);
        _updateTimer.Start();
    }

    ~DeviceDetailViewModel()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
    }

    private async Task LoadUserSettings()
    {
        var userSettings = await _dbContext.GetUserSettingsAsync();
        EmailAddress = userSettings?.EmailAddress ?? string.Empty;
    }

    partial void OnDeviceIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Task.Run(() => LoadDeviceDetailsAsync(value));
        }

        OnPropertyChanged(nameof(IsRemoveDeviceVisible));
    }

    [RelayCommand]
    private async Task ToggleStateAsync()
    {
        try
        {
            if (ConnectionState.ToLower() != "true")
            {
                ResponseMessage = "Failed to toggle device state. Make sure the device is connected and running.";
                return;
            }

            var newState = DeviceState == "On" ? "stop" : "start";
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId, newState);

            if (!response.Succeeded || response.Content is not CloudToDeviceMethodResult result || result.Status != 200)
            {
                ResponseMessage = "Failed to toggle device state. Make sure the device is connected and running.";
                Debug.WriteLine($"Error in ToggleStateAsync: {response.Message}");
                return;
            }

            await LoadDeviceDetailsAsync(DeviceId);
        }
        catch (Exception ex)
        {
            ResponseMessage = "Unable to toggle the device state. Please check if the device is online and try again.";
            Debug.WriteLine($"Error in ToggleStateAsync: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "connect");

            if (response.Succeeded && response.Content is CloudToDeviceMethodResult result && result.Status == 200)
            {
                await LoadDeviceDetailsAsync(DeviceId);
            }
            else
            {
                ResponseMessage = "Failed to connect the device. Make sure the device is reachable.";
                Debug.WriteLine($"Error in ConnectAsync: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            ResponseMessage = "Unable to connect to the device. Please check if the device is online and try again.";
            Debug.WriteLine($"Error in ConnectAsync: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        try
        {
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "disconnect");

            if (response.Succeeded && response.Content is CloudToDeviceMethodResult result && result.Status == 200)
            {
                await LoadDeviceDetailsAsync(DeviceId);
            }
            else
            {
                ResponseMessage = "Failed to disconnect the device. Make sure the device is reachable.";
                Debug.WriteLine($"Error in DisconnectAsync: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            ResponseMessage = "Unable to disconnect the device. Please check if the device is online and try again.";
            Debug.WriteLine($"Error in DisconnectAsync: {ex.Message}");
        }
    }

    public async Task LoadDeviceDetailsAsync(string deviceId)
    {
        if (_isRemovingDevice)
        {
            return;
        }

        DeviceId = deviceId;
        try
        {
            var response = await _deviceManager.GetDeviceTwinAsync(DeviceId);

            if (response.Succeeded && response.Content is Twin twin)
            {
                ConnectionState = twin.Properties.Reported.Contains("connectionState")
                    ? twin.Properties.Reported["connectionState"].ToString()
                    : (twin.Properties.Desired.Contains("connectionState")
                        ? twin.Properties.Desired["connectionState"].ToString()
                        : "Unknown");

                DeviceName = twin.Properties.Reported.Contains("deviceName")
                    ? twin.Properties.Reported["deviceName"].ToString()
                    : (twin.Properties.Desired.Contains("deviceName")
                        ? twin.Properties.Desired["deviceName"].ToString()
                        : "Unknown");

                DeviceType = twin.Properties.Reported.Contains("deviceType")
                    ? twin.Properties.Reported["deviceType"].ToString()
                    : (twin.Properties.Desired.Contains("deviceType")
                        ? twin.Properties.Desired["deviceType"].ToString()
                        : "Unknown");

                DeviceState = twin.Properties.Reported.Contains("deviceState")
                    ? (bool)twin.Properties.Reported["deviceState"] ? "On" : "Off"
                    : (twin.Properties.Desired.Contains("deviceState")
                        ? (bool)twin.Properties.Desired["deviceState"] ? "On" : "Off"
                        : "Unknown");

                LastActivityTime = twin.LastActivityTime.HasValue
                    ? twin.LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "No Activity";

                await SaveDeviceSettingsToDatabase(twin);
            }
            else
            {
                ResponseMessage = "Failed to load device details.";
                Debug.WriteLine($"Error in LoadDeviceDetailsAsync: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            ResponseMessage = $"Unable to load the device details. Please check if the device is online and try again.";
            Debug.WriteLine($"Error in LoadDeviceDetailsAsync: {ex.Message}");
        }
    }

    private async Task SaveDeviceSettingsToDatabase(Twin twin)
    {
        var deviceSettings = await _dbContext.GetDeviceSettingsAsync(DeviceId);
        if (deviceSettings == null)
        {
            deviceSettings = new DeviceSettings
            {
                DeviceId = DeviceId,
                DeviceType = twin.Properties.Reported.Contains("deviceType") ? twin.Properties.Reported["deviceType"]?.ToString() : "Unknown",
                DeviceName = twin.Properties.Reported.Contains("deviceName") ? twin.Properties.Reported["deviceName"]?.ToString() : "Unknown",
                LastActivityTime = twin.LastActivityTime.HasValue ? twin.LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                DeviceState = twin.Properties.Reported.Contains("deviceState") && (bool)twin.Properties.Reported["deviceState"]
            };
            await _dbContext.SaveDeviceSettingsAsync(deviceSettings);
        }
        else
        {
            deviceSettings.DeviceType = twin.Properties.Reported.Contains("deviceType") ? twin.Properties.Reported["deviceType"]?.ToString() : "Unknown";
            deviceSettings.DeviceName = twin.Properties.Reported.Contains("deviceName") ? twin.Properties.Reported["deviceName"]?.ToString() : "Unknown";
            deviceSettings.LastActivityTime = twin.LastActivityTime.HasValue ? twin.LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null;
            deviceSettings.DeviceState = twin.Properties.Reported.Contains("deviceState") && (bool)twin.Properties.Reported["deviceState"];
            await _dbContext.SaveDeviceSettingsAsync(deviceSettings);
        }
    }

    [RelayCommand]
    private async Task RemoveDeviceAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                "No email address registered. Cannot remove device.",
                "Ok");
                return;
            }

            var confirmed = await Application.Current!.MainPage!.DisplayAlert(
                "Confirm",
                "Are you sure you want to remove this device?",
                "Yes",
                "No");

            if (!confirmed)
            {
                return;
            }

            _isRemovingDevice = true;

            var response = await _deviceManager.RemoveDeviceAsync(DeviceId, EmailAddress);

            await Application.Current!.MainPage!.DisplayAlert(
            "Success",
            $"Device {DeviceId} removed successfully.",
            "Ok");

            await _mainViewModel.LoadDevicesAsync();

            _isRemovingDevice = false;

            await Shell.Current.GoToAsync("///MainPage");

        }
        catch (Exception ex)
        {
            _isRemovingDevice = false;
            await Application.Current!.MainPage!.DisplayAlert(
            "Error",
            "Unable to remove the device. Please check the device status and try again.",
            "Ok");
            Debug.WriteLine($"Error in RemoveDeviceAsync: {ex.Message}");
        }
    }
}
