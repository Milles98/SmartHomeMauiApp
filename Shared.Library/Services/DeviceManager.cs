using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System.Text;

namespace Shared.Library.Services;

public class DeviceManager
{
	private RegistryManager _registryManager;
	private ServiceClient _serviceClient;
	private string _connectionString;

	public DeviceManager(string connectionString)
	{
		UpdateConnectionString(connectionString);
	}

	public void UpdateConnectionString(string connectionString)
	{
		_connectionString = connectionString;
		_registryManager = RegistryManager.CreateFromConnectionString(connectionString);
		_serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
	}

	public async Task<IEnumerable<Twin>> GetDevicesAsync(string query)
	{
		try
		{
			var q = _registryManager.CreateQuery(query);
			return await q.GetNextAsTwinAsync();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching devices: {ex.Message}");
			return new List<Twin>();
		}
	}

	public async Task<Twin> GetDeviceTwinAsync(string deviceId)
	{
		try
		{
			return await _registryManager.GetTwinAsync(deviceId);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching device twin: {ex.Message}");
			return null!;
		}
	}

	public async Task InvokeDirectMethodAsync(string deviceId, string methodName, string payload)
	{
		try
		{
			var methodInvocation = new CloudToDeviceMethod(methodName) { ResponseTimeout = TimeSpan.FromSeconds(30) };
			methodInvocation.SetPayloadJson(payload);

			var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
			Console.WriteLine($"Response status: {response.Status}, payload: {response.GetPayloadAsJson()}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error invoking direct method: {ex.Message}");
		}
	}

	public async Task SendCloudToDeviceMessageAsync(string deviceId, string messageContent)
	{
		try
		{
			var message = new Message(Encoding.UTF8.GetBytes(messageContent))
			{
				ContentType = "application/json",
				ContentEncoding = "utf-8"
			};

			await _serviceClient.SendAsync(deviceId, message);
			Console.WriteLine($"Message sent to device {deviceId}: {messageContent}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error sending cloud-to-device message: {ex.Message}");
		}
	}
}
