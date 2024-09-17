using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Services;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

[QueryProperty(nameof(DeviceId), "deviceId")]
[QueryProperty(nameof(EmailAddress), "emailAddress")]
public partial class DeviceDetailViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;
    private readonly MainViewModel _mainViewModel;

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

    private System.Timers.Timer _updateTimer;

    public bool IsRemoveDeviceVisible =>
        DeviceId != "new-fan-bd437070-7751-45ca-8040-d484cedd6201" &&
        DeviceId != "ac-3cea3c99-c45a-4f44-a8ea-1fb70b9d2dca" &&
        DeviceId != "new-lamp-33c0d9c6-66f2-4aa6-bef5-c3d4417bc74c";

    public DeviceDetailViewModel(DeviceManager deviceManager, MainViewModel mainViewModel)
    {
        _deviceManager = deviceManager;
        _mainViewModel = mainViewModel;

        ResponseMessage = string.Empty;

        _updateTimer = new System.Timers.Timer(5000);
        _updateTimer.Elapsed += async (sender, e) => await LoadDeviceDetailsAsync(DeviceId);
        _updateTimer.Start();
    }

    ~DeviceDetailViewModel()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
    }

    [RelayCommand]
    private async Task NavigateHomeAsync()
    {
        await Shell.Current.GoToAsync("//MainPage");
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
        DeviceId = deviceId;
        try
        {
            var response = await _deviceManager.GetDeviceTwinAsync(DeviceId);

            if (response.Succeeded && response.Content is Twin twin)
            {
                ConnectionState = twin.Properties.Reported.Contains("connectionState") ?
                    twin.Properties.Reported["connectionState"].ToString() : "Unknown";

                DeviceName = twin.Properties.Reported.Contains("deviceName") ?
                    twin.Properties.Reported["deviceName"].ToString() : "Unknown";

                LastActivityTime = twin.LastActivityTime.HasValue
                    ? twin.LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "No Activity";

                DeviceType = twin.Properties.Reported.Contains("deviceType")
                    ? twin.Properties.Reported["deviceType"].ToString()
                    : "Unknown";

                DeviceState = twin.Properties.Reported.Contains("deviceState")
                    ? (bool)twin.Properties.Reported["deviceState"] ? "On" : "Off"
                    : "Unknown";
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

    [RelayCommand]
    private async Task RemoveDeviceAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                ResponseMessage = "No email address registered. Cannot remove device.";
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

            var response = await _deviceManager.RemoveDeviceAsync(DeviceId, EmailAddress);

            if (!response.Succeeded)
            {
                ResponseMessage = $"Failed to remove device {DeviceId}.";
                Debug.WriteLine($"Error in RemoveDeviceAsync: {response.Message}");
                return;
            }

            ResponseMessage = response.Message ?? $"Device {DeviceId} removed successfully.";
            await _mainViewModel.SetDevicesAsync();
            await Shell.Current.GoToAsync($"///MainPage");
        }
        catch (Exception ex)
        {
            ResponseMessage = "Unable to remove the device. Please check the device status and try again.";
            Debug.WriteLine($"Error in RemoveDeviceAsync: {ex.Message}");
        }
    }

}
