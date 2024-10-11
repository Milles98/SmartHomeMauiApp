using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.Services;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

[QueryProperty(nameof(DeviceId), "deviceId")]
[QueryProperty(nameof(EmailAddress), "emailAddress")]
public partial class DeviceDetailViewModel : ObservableObject, IDisposable
{
    private readonly IDeviceManager _deviceManager;
    private readonly ISmarthomeContext _dbContext;
    private readonly INavigationService _navigationService;
    private readonly ITimerService _timerService;
    private bool _isRemovingDevice;
    private bool _disposed;

    [ObservableProperty]
    private string? _deviceId;

    [ObservableProperty]
    private string? _status;

    [ObservableProperty]
    private string? _connectionState;

    [ObservableProperty]
    private string? _lastActivityTime;

    [ObservableProperty]
    private string? _deviceState;

    [ObservableProperty]
    private string? _deviceType;

    [ObservableProperty]
    private string? _deviceName;

    [ObservableProperty]
    private string? _emailAddress;

    public bool IsRemoveDeviceVisible =>
        DeviceId != "fan-5f8521d6-bf2a-4322-bf9a-69cc70bf9148" &&
        DeviceId != "ac-3cea3c99-c45a-4f44-a8ea-1fb70b9d2dca" &&
        DeviceId != "new-lamp-33c0d9c6-66f2-4aa6-bef5-c3d4417bc74c";

    public DeviceDetailViewModel(
        IDeviceManager deviceManager,
        ISmarthomeContext dbContext,
        INavigationService navigationService,
        ITimerService timerService)
    {
        _deviceManager = deviceManager;
        _dbContext = dbContext;
        _navigationService = navigationService;
        _timerService = timerService;
    }

    public async Task InitializeAsync()
    {
        await LoadUserSettingsAsync();
        _timerService.Start(() => LoadDeviceDetailsAsync(DeviceId!).ConfigureAwait(false), 5000);
    }

    public async Task LoadUserSettingsAsync()
    {
        var userSettings = await _dbContext.GetUserSettingsAsync();
        EmailAddress = userSettings?.EmailAddress ?? string.Empty;
    }

    partial void OnDeviceIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Task.Run(() => LoadDeviceDetailsAsync(value));
        }

        OnPropertyChanged(nameof(IsRemoveDeviceVisible));
    }

    [RelayCommand]
    public async Task ToggleStateAsync()
    {
        try
        {
            if (!ConnectionState!.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                await _navigationService.ShowAlertAsync("Error", "Failed to toggle device state. Ensure the device connection is correct and the app is running.", "Ok");
                return;
            }

            var newState = DeviceState == "On" ? "stop" : "start";
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId!, newState);

            if (!response.Succeeded || response.Content is not CloudToDeviceMethodResult result || result.Status != 200)
            {
                Debug.WriteLine($"Error in ToggleStateAsync: {response.Message}");
                await _navigationService.ShowAlertAsync("Error", "Failed to toggle device state.", "Ok");
                return;
            }

            await LoadDeviceDetailsAsync(DeviceId!);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ToggleStateAsync: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        try
        {
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId!, "connect");

            if (response.Succeeded && response.Content is CloudToDeviceMethodResult result && result.Status == 200)
            {
                await LoadDeviceDetailsAsync(DeviceId!);
            }
            else
            {
                Debug.WriteLine($"Error in ConnectAsync: {response.Message}");
                await _navigationService.ShowAlertAsync("Error", "Failed to connect the device. Ensure the device is reachable.", "Ok");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ConnectAsync: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task DisconnectAsync()
    {
        try
        {
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId!, "disconnect");

            if (response.Succeeded && response.Content is CloudToDeviceMethodResult result && result.Status == 200)
            {
                await LoadDeviceDetailsAsync(DeviceId!);
            }
            else
            {
                Debug.WriteLine($"Error in DisconnectAsync: {response.Message}");
                await _navigationService.ShowAlertAsync("Error", "Failed to disconnect the device. Ensure the device is reachable.", "Ok");
            }
        }
        catch (Exception ex)
        {
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

                await SaveDeviceSettingsToDatabaseAsync(twin);
            }
            else
            {
                Debug.WriteLine($"Error in LoadDeviceDetailsAsync: {response.Message}");
                await _navigationService.ShowAlertAsync("Error", "Failed to load device details.", "Ok");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoadDeviceDetailsAsync: {ex.Message}");
        }
    }

    public async Task SaveDeviceSettingsToDatabaseAsync(Twin twin)
    {
        var deviceSettings = await _dbContext.GetDeviceSettingsAsync(DeviceId!);
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
    public async Task RemoveDeviceAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                await _navigationService.ShowAlertAsync("Error", "No email address registered. Cannot remove device.", "Ok");
                return;
            }

            var confirmed = await _navigationService.ShowConfirmationAsync("Confirm", "Are you sure you want to remove this device?", "Yes", "No");

            if (!confirmed)
            {
                return;
            }

            _timerService.Stop();
            _isRemovingDevice = true;

            var response = await _deviceManager.DeviceRemovalSendEmailAsync(DeviceId!, EmailAddress!);

            await _navigationService.ShowAlertAsync("Success", $"Device {DeviceId} removed successfully and email confirmation has been sent.", "Ok");

            _isRemovingDevice = false;
        }
        catch (Exception ex)
        {
            _isRemovingDevice = false;
            Debug.WriteLine($"Error in RemoveDeviceAsync: {ex.Message}");
        }
        finally
        {
            _timerService.Stop();
            await _navigationService.NavigateToAsync("///MainPage");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _timerService.Stop();
            }

            _disposed = true;
        }
    }
}
