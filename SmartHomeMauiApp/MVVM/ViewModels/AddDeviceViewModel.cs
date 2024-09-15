using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices.Shared;
using System.Collections.ObjectModel;
using Shared.Library.Services;
using CommunityToolkit.Mvvm.Messaging;
using Shared.Library.Models;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class AddDeviceViewModel : ObservableObject
{
    private readonly DeviceManager _deviceManager;

    public AddDeviceViewModel(DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
        AvailableDeviceTypes = new ObservableCollection<string> { "Fan", "Lamp", "Ac" };
    }

    [ObservableProperty]
    private ObservableCollection<string> _availableDeviceTypes;

    [ObservableProperty]
    private string _selectedDeviceType;

    [ObservableProperty]
    private string _deviceId;

    [RelayCommand]
    private async Task AddDeviceAsync()
    {
        if (string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(SelectedDeviceType))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        var success = await _deviceManager.AddDeviceAsync(DeviceId, SelectedDeviceType);

        if (success)
        {
            await Application.Current!.MainPage!.DisplayAlert("Success", "Device added successfully.", "OK");
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to add the device. It may already exist.", "OK");
        }
    }

    [RelayCommand]
    private void GenerateDeviceId()
    {
        DeviceId = Guid.NewGuid().ToString();
    }
}
