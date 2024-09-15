using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Services;
using System.Collections.ObjectModel;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;

    [ObservableProperty]
    private ObservableCollection<Twin> _devices = [];

    [ObservableProperty]
    private Twin _selectedDevice;

    public async Task SetDevicesAsync()
    {
        Devices = new ObservableCollection<Twin>(await _deviceManager.GetDevicesAsync("SELECT * FROM devices"));
    }

    private static readonly Dictionary<string, string> DeviceTypeToImageMap = new Dictionary<string, string>
    {
        { "Fan", "fan_icon.png" },
        { "Lamp", "maui_lamp.png" },
        { "Ac", "ac_icon.png" },
    };

    public string GetDeviceIcon(Twin twin)
    {
        if (twin.Properties.Reported.Contains("deviceType"))
        {
            string deviceType = twin.Properties.Reported["deviceType"].ToString();

            return DeviceTypeToImageMap.TryGetValue(deviceType, out var imageName) ? imageName : "microchip.png";
        }

        return "microchip.png";
    }

    [RelayCommand]
    private async Task NavigateToDeviceDetailAsync(Twin device)
    {
        if (device == null)
            return;


        var emailAddress = Preferences.Get("EmailAddress", string.Empty);

        await Shell.Current.GoToAsync($"///DeviceDetailPage?deviceId={device.DeviceId}&emailAddress={emailAddress}");
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await Shell.Current.GoToAsync("///SettingsPage");
    }

    [RelayCommand]
    private async Task NavigateToAddDeviceAsync()
    {
        await Shell.Current.GoToAsync("///AddDevicePage");
    }


    public MainViewModel(DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;

        Task.Run(SetDevicesAsync);
    }
}
