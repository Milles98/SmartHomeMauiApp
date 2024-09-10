using Azure;
using Azure.Communication.Email;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Mail;
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

		if (_serviceClient == null || _registryManager == null)
		{
			Debug.WriteLine("Error: ServiceClient or RegistryManager is not initialized.");
		}
		else
		{
			Debug.WriteLine("ServiceClient and RegistryManager are initialized successfully.");
		}
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

	public async Task<CloudToDeviceMethodResult> InvokeDirectMethodAsync(string deviceId, string methodName, object? payload = null, int responseTimeoutSeconds = 30)
	{
		try
		{
			var cloudMethod = new CloudToDeviceMethod(methodName)
			{
				ResponseTimeout = TimeSpan.FromSeconds(responseTimeoutSeconds)
			};

			if (payload != null)
			{
				cloudMethod.SetPayloadJson(JsonConvert.SerializeObject(payload));
			}

			var result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, cloudMethod);
			if (result != null)
			{
				return result;
			}
		}
		catch (DeviceNotFoundException ex)
		{
			Debug.WriteLine("Device is not available or disconnected.");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error invoking direct method: {ex.Message}");
			Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
			if (ex.InnerException != null)
			{
				Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
				Debug.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
			}
		}

		return null!;
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

	public async Task<bool> RemoveDeviceAsync(string deviceId, string emailAddress)
	{
		if (string.IsNullOrWhiteSpace(emailAddress))
		{
			Console.WriteLine("Email address cannot be empty.");
			return false;
		}

		try
		{
			await _registryManager.RemoveDeviceAsync(deviceId);
			Console.WriteLine($"Device {deviceId} has been removed.");

			var emailClient = new EmailClient("endpoint=https://millesemailservice.europe.communication.azure.com/;accesskey=98Ku6INb6lIaWngj70BEnb2R0mB57HtsfdiqI2sfmNqPDhnBvLdKJQQJ99AIACULyCpq7IbPAAAAAZCSg1Jp");

			var emailContent = new EmailContent("Device Removal Confirmation")
			{
				PlainText = $"The device with ID {deviceId} has been successfully removed from Azure IoT Hub.",
			};

			var emailRecipients = new EmailRecipients(new List<EmailAddress>
		{
			new EmailAddress(emailAddress)
		});

			var emailMessage = new EmailMessage(
				"DoNotReply@39798093-33ea-4abd-9e0d-401459f2e05a.azurecomm.net",
				emailRecipients,
				emailContent
			);

			var response = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);

			if (response.Value.Status == EmailSendStatus.Succeeded)
			{
				Console.WriteLine("Confirmation email sent successfully.");
			}
			else
			{
				Console.WriteLine($"Failed to send confirmation email. Status: {response.Value.Status}");
			}

			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error removing device: {ex.Message}");
			return false;
		}
	}

}
