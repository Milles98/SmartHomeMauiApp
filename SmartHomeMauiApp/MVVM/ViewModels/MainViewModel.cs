using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Azure.Devices.Shared;
using Shared.Library.Services;
using System.Collections.ObjectModel;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class MainViewModel : ObservableObject
{
	private readonly DeviceManager _deviceManager;

	[ObservableProperty]
	private ObservableCollection<Twin> _devices = [];

	private async Task SetDevicesAsync()
	{
		Devices = new ObservableCollection<Twin>(await _deviceManager.GetDevicesAsync("SELECT * FROM devices"));
	}

	public MainViewModel(DeviceManager deviceManager)
	{
		_deviceManager = deviceManager;
	}
}
