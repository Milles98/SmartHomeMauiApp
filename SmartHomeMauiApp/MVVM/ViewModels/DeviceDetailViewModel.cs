using Azure.Communication.Email;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Devices.Common.Exceptions;
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
	private string _fanState;

	[ObservableProperty]
	private string _lampState;

	[ObservableProperty]
	private string _temperatureValue;

	[ObservableProperty]
	private string _deviceType;

	[ObservableProperty]
	private string _emailAddress;

	private System.Timers.Timer _updateTimer;

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
	}

	[RelayCommand]
	private async Task ToggleFanStateAsync()
	{
		try
		{
			var result = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "ToggleFan");

			if (result != null && result.Status == 200)
			{
				await LoadDeviceDetailsAsync(DeviceId);
			}
			else
			{
				await Application.Current!.MainPage!.DisplayAlert(
					"Error",
					"Failed to toggle fan state, have you started the WPF application?",
					"OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current!.MainPage!.DisplayAlert(
				"Error",
				$"Failed to toggle fan state: {ex.Message}",
				"OK");
		}
	}

	[RelayCommand]
	private async Task ToggleLampStateAsync()
	{
		try
		{
			var result = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "ToggleLamp");

			if (result != null && result.Status == 200)
			{
				await LoadDeviceDetailsAsync(DeviceId);
			}
			else
			{
				await Application.Current!.MainPage!.DisplayAlert(
					"Error",
					"Failed to toggle lamp state. Please check if the device is connected and running.",
					"OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current!.MainPage!.DisplayAlert(
				"Error",
				$"Failed to toggle lamp state: {ex.Message}",
				"OK");
		}
	}

	[RelayCommand]
	private async Task ToggleTemperatureStateAsync()
	{
		try
		{
			var result = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "ToggleTemperature");

			if (result != null && result.Status == 200)
			{
				await LoadDeviceDetailsAsync(DeviceId);
			}
			else
			{
				await Application.Current!.MainPage!.DisplayAlert(
					"Error",
					"Failed to toggle temperature state, have you started the WPF application?",
					"OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current!.MainPage!.DisplayAlert(
				"Error",
				$"Failed to toggle temperature state: {ex.Message}",
				"OK");
		}
	}

	public async Task LoadDeviceDetailsAsync(string deviceId)
	{
		DeviceId = deviceId;
		try
		{
			var twin = await _deviceManager.GetDeviceTwinAsync(DeviceId);

			if (twin != null)
			{
				Status = twin.Status.ToString();
				ConnectionState = twin.ConnectionState.ToString();

				LastActivityTime = twin.LastActivityTime.HasValue
					? twin.LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
					: "No Activity";

				DeviceType = twin.Properties.Reported.Contains("DeviceType")
					? twin.Properties.Reported["DeviceType"].ToString()
					: "Unknown";

				switch (DeviceType)
				{
					case "Fan":
						if (twin.Properties.Reported.Contains("fanState"))
						{
							FanState = twin.Properties.Reported["fanState"]?.ToString();
						}
						break;

					case "Lamp":
						if (twin.Properties.Reported.Contains("lampState"))
						{
							LampState = twin.Properties.Reported["lampState"]?.ToString();
						}
						break;

					case "TemperatureSensor":
						if (twin.Properties.Reported.Contains("temperature"))
						{
							TemperatureValue = twin.Properties.Reported["temperature"]?.ToString();
						}
						break;

					default:
						Console.WriteLine("Unknown Device Type");
						break;
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching device details: {ex.Message}");
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
			bool result = await _deviceManager.RemoveDeviceAsync(DeviceId, EmailAddress);
			if (result)
			{
				await Application.Current.MainPage.DisplayAlert(
					"Success",
					$"Device {DeviceId} removed successfully.",
					"OK");

				await _mainViewModel.SetDevicesAsync();

				await Shell.Current.GoToAsync($"///MainPage");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert(
					"Error",
					$"Failed to remove device {DeviceId}.",
					"OK");
			}
		}
	}
}
