using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Models.IotResources;
using Shared.Library.Services;
using SmartHomeMauiApp.Services;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class AddDeviceViewModel : ObservableObject
{
    private readonly IDeviceManager _deviceManager;
    private readonly INavigationService _navigationService;
    private readonly HttpClient _httpClient;

    public AddDeviceViewModel(IDeviceManager deviceManager, INavigationService navigationService, HttpClient httpClient)
    {
        _deviceManager = deviceManager;
        _navigationService = navigationService;
        _httpClient = httpClient;

        AvailableDeviceTypes = new ObservableCollection<string> { "Fan", "Lamp", "Ac" };
        ResponseMessage = string.Empty;
    }

    [ObservableProperty]
    private ObservableCollection<string> _availableDeviceTypes;

    [ObservableProperty]
    private string? _selectedDeviceType;

    [ObservableProperty]
    private string? _deviceId;

    [ObservableProperty]
    private string? _deviceName;

    [ObservableProperty]
    private string _responseMessage;

    [ObservableProperty]
    private string _responseMessageColor = "Red";

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    public async Task AddDeviceAsync()
    {
        if (string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(DeviceName) || string.IsNullOrEmpty(SelectedDeviceType))
        {
            ResponseMessage = "Please enter device name, select device type, and generate id";
            return;
        }

        try
        {
            IsBusy = true;

            var drr = new DeviceRegistrationRequest
            {
                DeviceId = DeviceId,
                DeviceName = DeviceName,
                DeviceType = SelectedDeviceType
            };

            var result = await _httpClient.PostAsJsonAsync("https://mille-azure-function.azurewebsites.net/api/DeviceRegistration?code=2kQ4cfP0Og_O7tqe60ZJJ7yS63aP0ocoNen0fpeW5AvoAzFuR6vgEg%3D%3D", drr);

            if (result.IsSuccessStatusCode)
            {
                ResponseMessage = "Device added successfully!";
                ResponseMessageColor = "Green";
                DeviceId = string.Empty;
                DeviceName = string.Empty;
                SelectedDeviceType = string.Empty;

                await Task.Delay(2000);

                await _navigationService.NavigateToAsync("///MainPage");
            }
            else
            {
                ResponseMessage = $"Failed to register device. Status Code: {result.StatusCode}";
                ResponseMessageColor = "Red";
            }
        }
        catch (Exception ex)
        {
            ResponseMessage = $"An error occurred: {ex.Message}";
            ResponseMessageColor = "Red";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void GenerateDeviceId()
    {
        DeviceId = Guid.NewGuid().ToString();
    }
}
