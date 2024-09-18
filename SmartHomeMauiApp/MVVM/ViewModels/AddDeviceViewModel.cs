using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
    private string _responseMessage;

    [RelayCommand]
    private async Task AddDeviceAsync()
    {
        if (string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(SelectedDeviceType))
        {
            ResponseMessage = "Please select device type and generate id";
            return;
        }

        try
        {
            var response = await _deviceManager.AddDeviceAsync(DeviceId, SelectedDeviceType);

            if (response.Succeeded)
            {
                ResponseMessage = response.Message ?? "Device added successfully.";
                await _mainViewModel.LoadDevicesAsync();
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ResponseMessage = "Failed to add the device. It may already exist.";
            }
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
