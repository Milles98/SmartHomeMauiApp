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
	private SettingsViewModel _settingsViewModel;

	[ObservableProperty]
	private Twin _selectedDevice;

	private async Task SetDevicesAsync()
	{
		Devices = new ObservableCollection<Twin>(await _deviceManager.GetDevicesAsync("SELECT * FROM devices"));
	}

	[RelayCommand]
	private async Task ToggleDeviceStateAsync()
	{
		if (SelectedDevice == null)
		{
			Console.WriteLine("No device selected.");
			return;
		}

		// Skicka Direct Method till den valda enheten utan att ange någon specifik payload
		await _deviceManager.InvokeDirectMethodAsync(SelectedDevice.DeviceId, "ToggleFan", "{}");
	}


	public MainViewModel(DeviceManager deviceManager)
	{
		_deviceManager = deviceManager;

		_settingsViewModel = new SettingsViewModel(deviceManager);

		Task.Run(SetDevicesAsync);
	}
}
