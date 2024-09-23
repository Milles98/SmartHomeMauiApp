using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Shared.Library.Models;
using Shared.Library.Models.WeatherApi;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;
    private readonly DbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<Twin> _devices = [];

    [ObservableProperty]
    private Twin? _selectedDevice;

    [ObservableProperty]
    private string _responseMessage;

    [ObservableProperty]
    private string weatherIcon;

    [ObservableProperty]
    private string temperature;

    [ObservableProperty]
    private string conditionText;

    public MainViewModel(DeviceManager deviceManager, DbContext dbContext)
    {
        _deviceManager = deviceManager;
        _dbContext = dbContext;

        ResponseMessage = string.Empty;

        LoadDevicesAsync().ConfigureAwait(false);
        LoadWeatherDataAsync().ConfigureAwait(false);
    }

    private async Task LoadWeatherDataAsync()
    {
        try
        {
            using var client = new HttpClient();
            string url = "http://api.weatherapi.com/v1/current.json?key=abe8320defb74b72adf111532242504&q=Stockholm&aqi=no";
            var response = await client.GetStringAsync(url);

            var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(response);

            WeatherIcon = $"https:{weatherData!.Current!.Condition!.Icon}";
            Temperature = $"{weatherData.Current.TempC}°C";
            ConditionText = weatherData.Current.Condition.Text!;
        }
        catch (Exception ex)
        {
            ResponseMessage = "Unable to display weather";
            Debug.WriteLine($"Error {ex.Message}");
        }
    }

    public async Task LoadDevicesAsync()
    {
        var response = await _deviceManager.GetDevicesAsync("SELECT * FROM devices");

        if (response.Succeeded && response.Content is IEnumerable<Twin> devices)
        {
            Devices = new ObservableCollection<Twin>(devices);
            OnPropertyChanged(nameof(Devices));

            foreach (var device in devices)
            {

                bool.TryParse(device.Properties.Reported.Contains("connectionState").ToString(), out var connectionState);

                var deviceSettings = new DeviceSettings
                {
                    DeviceId = device.DeviceId,
                    DeviceType = device.Properties.Reported.Contains("deviceType") ? device.Properties.Reported["deviceType"].ToString() : "Unknown",
                    DeviceName = device.Properties.Reported.Contains("deviceName") ? device.Properties.Reported["deviceName"].ToString() : "Unknown",
                    IsConnected = connectionState,
                };

                await _dbContext.SaveDeviceSettingsAsync(deviceSettings);
            }
        }
        else
        {
            ResponseMessage = "Failed to retrieve devices.";
            Debug.WriteLine($"Error in SetDevicesAsync: {response.Message}");
        }
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


        var userSettings = await _dbContext.GetUserSettingsAsync();
        var emailAddress = userSettings?.EmailAddress ?? string.Empty;

        await Shell.Current.GoToAsync($"///DeviceDetailPage?deviceId={device.DeviceId}&emailAddress={emailAddress}");
    }
}
