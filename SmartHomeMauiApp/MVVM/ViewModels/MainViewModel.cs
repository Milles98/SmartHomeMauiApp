using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IDeviceManager _deviceManager;
    private readonly ISmarthomeContext _dbContext;
    private readonly IWeatherService _weatherService;

    [ObservableProperty]
    private ObservableCollection<Twin> _devices = [];

    [ObservableProperty]
    private string _time;

    [ObservableProperty]
    private Twin? _selectedDevice;

    [ObservableProperty]
    private string? _weatherIcon;

    [ObservableProperty]
    private string? _temperature;

    [ObservableProperty]
    private string? _conditionText;

    [ObservableProperty]
    private bool _isWeatherVisible = false;

    public MainViewModel(IDeviceManager deviceManager, ISmarthomeContext dbContext, IWeatherService weatherService, INavigationService navigationService)
    {
        _deviceManager = deviceManager;
        _dbContext = dbContext;
        _weatherService = weatherService;
        _navigationService = navigationService;

        LoadDevicesAsync().ConfigureAwait(false);
        LoadWeatherDataAsync().ConfigureAwait(false);

        Time = DateTime.Now.ToString("HH:mm");
        StartTimer();
    }

    private void StartTimer()
    {
        var timer = new System.Timers.Timer(10000);
        timer.Elapsed += (sender, e) => Time = DateTime.Now.ToString("HH:mm");
        timer.Start();
    }

    public async Task LoadWeatherDataAsync()
    {
        var weatherData = await _weatherService.GetWeatherAsync("Stockholm");

        if (weatherData != null)
        {
            WeatherIcon = $"https:{weatherData.Current!.Condition!.Icon}";
            Temperature = $"{(int)weatherData.Current.TempC}°C";
            ConditionText = weatherData.Current.Condition.Text!;
            IsWeatherVisible = true;
        }
        else
        {
            IsWeatherVisible = false;
            Debug.WriteLine("Error loading weather data");
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
            Debug.WriteLine($"Error in SetDevicesAsync: {response.Message}");
        }
    }

    public static readonly Dictionary<string, string> DeviceTypeToImageMap = new Dictionary<string, string>
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
    public async Task NavigateToDeviceDetailAsync(Twin device)
    {
        if (device == null)
            return;

        var userSettings = await _dbContext.GetUserSettingsAsync();
        var emailAddress = userSettings?.EmailAddress ?? string.Empty;

        await _navigationService.NavigateToAsync($"///DeviceDetailPage?deviceId={device.DeviceId}&emailAddress={emailAddress}");
    }
}
