using CommunityToolkit.Mvvm.ComponentModel;
using Shared.Library.Services;

namespace SmartHomeMauiApp.MVVM.ViewModels;

[QueryProperty(nameof(DeviceId), "deviceId")]
public partial class DeviceDetailViewModel : ObservableObject
{
	private readonly DeviceManager _deviceManager;

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

	public DeviceDetailViewModel(DeviceManager deviceManager)
	{
		_deviceManager = deviceManager;
	}

	partial void OnDeviceIdChanged(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			// Ladda detaljer om enheten när DeviceId är satt
			Task.Run(() => LoadDeviceDetailsAsync(value));
		}
	}

	public async Task LoadDeviceDetailsAsync(string deviceId)
	{
		DeviceId = deviceId;
		try
		{
			// Använd DeviceManager för att hämta enhetens tvilling (twin)
			var twin = await _deviceManager.GetDeviceTwinAsync(DeviceId);

			if (twin != null)
			{
				// Uppdatera egenskaper för UI
				Status = twin.Status.ToString();
				ConnectionState = twin.ConnectionState.ToString();

				// Kontrollera om LastActivityTime har ett värde innan konvertering
				LastActivityTime = twin.LastActivityTime.HasValue
					? twin.LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
					: "No Activity"; // Standardvärde om det är null

				FanState = twin.Properties.Reported["fanState"]?.ToString();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching device details: {ex.Message}");
		}
	}

}
