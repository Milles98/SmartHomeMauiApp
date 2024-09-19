using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Shared.Library.Models.IotResources;
using Shared.Library.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class AddDeviceViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;
    private readonly MainViewModel _mainViewModel;

    public AddDeviceViewModel(DeviceManager deviceManager, MainViewModel viewModel)
    {
        _deviceManager = deviceManager;
        _mainViewModel = viewModel;

        AvailableDeviceTypes = new ObservableCollection<string> { "Fan", "Lamp", "Ac" };
        ResponseMessage = string.Empty;
    }

    [ObservableProperty]
    private ObservableCollection<string> _availableDeviceTypes;

    [ObservableProperty]
    private string _selectedDeviceType;

    [ObservableProperty]
    private string _deviceId;

    [ObservableProperty]
    private string _deviceName;

    [ObservableProperty]
    private string _responseMessage;

    [RelayCommand]
    private async Task AddDeviceAsync()
    {
        if (string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(DeviceName) || string.IsNullOrEmpty(SelectedDeviceType))
        {
            ResponseMessage = "Please enter device name, select device type, and generate id";
            return;
        }

        try
        {
            var drr = new DeviceRegistrationRequest
            {
                DeviceId = DeviceId,
                DeviceName = DeviceName,
                DeviceType = SelectedDeviceType
            };

            using var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            var result = await http.PostAsJsonAsync("https://mille-azure-function.azurewebsites.net/api/DeviceRegistration?code=2kQ4cfP0Og_O7tqe60ZJJ7yS63aP0ocoNen0fpeW5AvoAzFuR6vgEg%3D%3D", drr);

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<DeviceRegistrationResponse>(content);

                if (response == null)
                {
                    Debug.WriteLine("Deserialization failed or response is null.");
                }
                else
                {
                    Debug.WriteLine(response.ConnectionString ?? "ConnectionString is null");
                    Debug.WriteLine(response.DeviceName ?? "DeviceName is null");
                }
            }
            else
            {
                Debug.WriteLine($"Failed to register device. Status Code: {result.StatusCode}");
                ResponseMessage = $"Failed to register device. Status Code: {result.StatusCode}";
            }


            _mainViewModel.OnDeviceAdded?.Invoke();

            await _mainViewModel.LoadDevicesAsync();
            await Shell.Current.GoToAsync("//MainPage");

            //Andra försök
            //var deviceInstance = await _deviceManager.RegisterDeviceAsync(DeviceId, SelectedDeviceType);
            //await _mainViewModel.LoadDevicesAsync();
            //await Shell.Current.GoToAsync("//MainPage");


            //Första försök
            //var response = await _deviceManager.AddDeviceAsync(DeviceId, SelectedDeviceType);

            //if (response.Succeeded)
            //{
            //    ResponseMessage = response.Message ?? "Device added successfully.";
            //    await _mainViewModel.LoadDevicesAsync();
            //    await Shell.Current.GoToAsync("//MainPage");
            //}
            //else
            //{
            //    ResponseMessage = "Failed to add the device. It may already exist.";
            //}
        }
        catch (Exception ex)


        {
            ResponseMessage = $"An error occurred: {ex.Message}";
            Debug.WriteLine($"Error in AddDeviceAsync: {ex}");
        }
    }

    [RelayCommand]
    private void GenerateDeviceId()
    {
        DeviceId = Guid.NewGuid().ToString();
    }

    [RelayCommand]
    private async Task NavigateHomeAsync()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
