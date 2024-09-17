using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Services;

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

    private System.Timers.Timer _updateTimer;

    public bool IsRemoveDeviceVisible =>
        DeviceId != "new-fan-bd437070-7751-45ca-8040-d484cedd6201" &&
        DeviceId != "ac-3cea3c99-c45a-4f44-a8ea-1fb70b9d2dca" &&
        DeviceId != "new-lamp-33c0d9c6-66f2-4aa6-bef5-c3d4417bc74c";

    public DeviceDetailViewModel(DeviceManager deviceManager, MainViewModel mainViewModel)
    {
        _deviceManager = deviceManager;
        _mainViewModel = mainViewModel;

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
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    "Failed to toggle device state. Make sure the device is connected and running.",
                    "OK");
                return;
            }

            var newState = DeviceState == "On" ? "stop" : "start";
            var response = await _deviceManager.InvokeDirectMethodAsync(DeviceId, newState);

            if (!response.Succeeded || response.Content is not CloudToDeviceMethodResult result || result.Status != 200)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    response.Message ?? "Failed to toggle device state. Make sure the device is connected and running.",
                    "OK");
                return;
            }

            await LoadDeviceDetailsAsync(DeviceId);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"Failed to toggle device state: {ex.Message}",
                "OK");
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
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    response.Message ?? "Failed to connect the device. Make sure the device is reachable.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"Failed to connect the device: {ex.Message}",
                "OK");
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
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    response.Message ?? "Failed to disconnect the device. Make sure the device is reachable.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"Failed to disconnect the device: {ex.Message}",
                "OK");
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
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    response.Message ?? "Failed to load device details.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"Error fetching device details: {ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    private async Task RemoveDeviceAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailAddress))
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                "No email address registered. Cannot remove device.",
                "OK");
            return;
        }

        var confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm",
            "Are you sure you want to remove this device?",
            "Yes",
            "No");

        if (confirmed)
        {
            var response = await _deviceManager.RemoveDeviceAsync(DeviceId, EmailAddress);

            if (response.Succeeded)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    response.Message ?? $"Device {DeviceId} removed successfully.",
                    "OK");

                await _mainViewModel.SetDevicesAsync();

                await Shell.Current.GoToAsync($"///MainPage");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    response.Message ?? $"Failed to remove device {DeviceId}.",
                    "OK");
            }
        }
    }
}
