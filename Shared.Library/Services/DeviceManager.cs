using Azure;
using Azure.Communication.Email;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Shared.Library.Models;
using Shared.Library.Models.IotResources;
using System.Diagnostics;

namespace Shared.Library.Services;

public class DeviceManager
{
    private readonly string? _connectionString;
    private RegistryManager? _registryManager;
    private ServiceClient? _serviceClient;

    public DeviceManager(string? connectionString)
    {
        _connectionString = connectionString;
        UpdateConnectionString(_connectionString!);
    }

    public bool Disconnect()
    {
        try
        {
            _registryManager!.Dispose();
            _serviceClient!.Dispose();

            if (_registryManager == null && _serviceClient == null)
                return true;

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error disconnecting: {ex.Message}");
            return false;
        }
    }

    public async Task<DeviceInstance> RegisterDeviceAsync(string deviceId, string deviceName, string deviceType)
    {
        if (string.IsNullOrEmpty(deviceId))
            return null!;

        var deviceInstance = new DeviceInstance
        {
            Device = await _registryManager!.GetDeviceAsync(deviceId) ?? await _registryManager.AddDeviceAsync(new Device(deviceId))
        };

        var desiredProperties = new Dictionary<string, string>
        {
            { "deviceName", deviceName },
            { "deviceType", deviceType },
            { "connectionState", "false" },
            { "deviceState", "false" }
        };

        Debug.WriteLine($"Updating twin for {deviceId} with deviceType: {deviceType}");

        await UpdateDesiredPropertiesAsync(deviceInstance.Device, desiredProperties);

        deviceInstance.ConnectionString = GetDeviceConnectionString(deviceInstance.Device);
        deviceInstance.Twin = await _registryManager.GetTwinAsync(deviceInstance.Device.Id);

        return deviceInstance;
    }


    public string GetDeviceConnectionString(Device device)
    {
        var deviceConnectionString = $"{_connectionString!.Split(";")[0]};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";
        return deviceConnectionString ?? null!;
    }

    public async Task<bool> UpdateDesiredPropertiesAsync(Device device, Dictionary<string, string> desiredProperties)
    {
        try
        {
            var twin = await _registryManager!.GetTwinAsync(device.Id);

            foreach (var property in desiredProperties)
            {
                twin.Properties.Desired[property.Key] = property.Value;
            }

            await _registryManager.UpdateTwinAsync(device.Id, twin, twin.ETag);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating desired properties: {ex.Message}");
            return false;
        }
    }

    public void UpdateConnectionString(string connectionString)
    {
        _registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

        if (_serviceClient == null || _registryManager == null)
            Debug.WriteLine("Error: ServiceClient or RegistryManager is not initialized.");
        else
            Debug.WriteLine("ServiceClient and RegistryManager are initialized successfully.");
    }

    public async Task<ResponseResult> GetDevicesAsync(string query)
    {
        try
        {
            var q = _registryManager?.CreateQuery(query);
            var devices = await q.GetNextAsTwinAsync();
            return new ResponseResult { Succeeded = true, Content = devices };
        }
        catch (Exception ex)
        {
            return new ResponseResult { Succeeded = false, Message = $"Error fetching devices: {ex.Message}" };
        }
    }

    public async Task<ResponseResult> GetDeviceTwinAsync(string deviceId)
    {
        try
        {
            var twin = await _registryManager!.GetTwinAsync(deviceId);
            return new ResponseResult { Succeeded = true, Content = twin };
        }
        catch (Exception ex)
        {
            return new ResponseResult { Succeeded = false, Message = $"Error fetching device twin: {ex.Message}" };
        }
    }

    public async Task<ResponseResult> InvokeDirectMethodAsync(string deviceId, string methodName, object? payload = null, int responseTimeoutSeconds = 30)
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

            var result = await _serviceClient!.InvokeDeviceMethodAsync(deviceId, cloudMethod);
            return new ResponseResult { Succeeded = true, Content = result };
        }
        catch (Exception ex)
        {
            return new ResponseResult { Succeeded = false, Message = $"Error invoking direct method: {ex.Message}" };
        }
    }

    public async Task<ResponseResult> DeviceRemovalSendEmailAsync(string deviceId, string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrEmpty(deviceId))
            return new ResponseResult { Succeeded = false, Message = "Email address and device id cannot be empty." };

        try
        {
            await _registryManager!.RemoveDeviceAsync(deviceId);

            var emailClient = new EmailClient("endpoint=https://millesemailservice.europe.communication.azure.com/;accesskey=98Ku6INb6lIaWngj70BEnb2R0mB57HtsfdiqI2sfmNqPDhnBvLdKJQQJ99AIACULyCpq7IbPAAAAAZCSg1Jp");

            var emailContent = new EmailContent("Iot Device Removal Confirmation")
            {
                PlainText = $"The Iot device with ID {deviceId} has been successfully removed from Azure IoT Hub.",
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

            return new ResponseResult
            {
                Succeeded = response.Value.Status == EmailSendStatus.Succeeded,
                Message = response.Value.Status == EmailSendStatus.Succeeded
                    ? "Confirmation email sent successfully."
                    : $"Failed to send confirmation email. Status: {response.Value.Status}"
            };
        }
        catch (Exception ex)
        {
            return new ResponseResult { Succeeded = false, Message = $"Error removing device: {ex.Message}" };
        }
    }
}