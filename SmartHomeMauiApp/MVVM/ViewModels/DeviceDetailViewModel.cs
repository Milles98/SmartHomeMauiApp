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
	private async Task ConnectDeviceAsync()
	{
		try
		{
			var result = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "ConnectDevice");

			if (result != null && result.Status == 200)
			{
				await LoadDeviceDetailsAsync(DeviceId);
				ConnectionState = "Connected";
			}
			else
			{
				await Application.Current!.MainPage!.DisplayAlert(
					"Error",
					"Failed to connect the device, have you started the WPF application?",
					"OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current!.MainPage!.DisplayAlert(
				"Error",
				$"Failed to connect the device: {ex.Message}",
				"OK");
		}
	}

	[RelayCommand]
	private async Task DisconnectDeviceAsync()
	{
		try
		{
			var result = await _deviceManager.InvokeDirectMethodAsync(DeviceId, "DisconnectDevice");

			if (result != null && result.Status == 200)
			{
				await LoadDeviceDetailsAsync(DeviceId);
				ConnectionState = "Disconnected";
			}
			else
			{
				await Application.Current!.MainPage!.DisplayAlert(
					"Error",
					"Failed to disconnect the device.",
					"OK");
			}
		}
		catch (DeviceNotFoundException)
		{
			await Application.Current!.MainPage!.DisplayAlert(
				"Info",
				"Device is already disconnected or not available.",
				"OK");
		}
		catch (Exception ex)
		{
			await Application.Current!.MainPage!.DisplayAlert(
				"Error",
				$"Failed to disconnect the device: {ex.Message}",
				"OK");
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
	private async Task LoadDeviceDetailsAsync()
	{
		await LoadDeviceDetailsAsync(DeviceId);
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

				FanState = twin.Properties.Reported["fanState"]?.ToString();
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
