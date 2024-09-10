using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Services;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
	private readonly DeviceManager _deviceManager;

	[ObservableProperty]
	private string _connectionString;

	[ObservableProperty]
	private string _emailAddress;

	public SettingsViewModel(DeviceManager deviceManager)
	{
		_deviceManager = deviceManager;
		ConnectionString = "HostName=Milles-IoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4o/msHXU6XCzmeL9Jazb6eKlPZJbf6D4KAIoTFqR/EI="; // Sätt en standard connection string om det behövs
	}

	[RelayCommand]
	private async Task SaveSettingsAsync()
	{
		if (string.IsNullOrWhiteSpace(ConnectionString))
		{
			await Application.Current!.MainPage!.DisplayAlert(
					"Error",
					"Connection String cannot be empty.",
					"OK");
			return;
		}

		_deviceManager.UpdateConnectionString(ConnectionString);

		await Application.Current!.MainPage!.DisplayAlert(
					"Success",
					"Settings have been saved and IoT Hub connection has been updated.",
					"OK");

		Preferences.Set("EmailAddress", EmailAddress);

		await Task.CompletedTask;
	}
}
