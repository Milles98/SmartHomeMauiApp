using Azure;
using Azure.Communication.Email;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Shared.Library.Models;
using System.Diagnostics;
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

    public async Task<ResponseResult> GetDevicesAsync(string query)
    {
        try
        {
            var q = _registryManager.CreateQuery(query);
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
            var twin = await _registryManager.GetTwinAsync(deviceId);
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

            var result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, cloudMethod);
            return new ResponseResult { Succeeded = true, Content = result };
        }
        catch (Exception ex)
        {
            return new ResponseResult { Succeeded = false, Message = $"Error invoking direct method: {ex.Message}" };
        }
    }

    //public async Task<ResponseResult> SendCloudToDeviceMessageAsync(string deviceId, string messageContent)
    //{
    //    try
    //    {
    //        var message = new Message(Encoding.UTF8.GetBytes(messageContent))
    //        {
    //            ContentType = "application/json",
    //            ContentEncoding = "utf-8"
    //        };

    //        await _serviceClient.SendAsync(deviceId, message);
    //        return new ResponseResult { Succeeded = true, Message = $"Message sent to device {deviceId}: {messageContent}" };
    //    }
    //    catch (Exception ex)
    //    {
    //        return new ResponseResult { Succeeded = false, Message = $"Error sending cloud-to-device message: {ex.Message}" };
    //    }
    //}

    public async Task<ResponseResult> AddDeviceAsync(string deviceId, string deviceType)
    {
        try
        {
            var device = new Device(deviceId);
            await _registryManager.AddDeviceAsync(device);

            var twin = new Twin
            {
                Properties =
                    {
                        Desired =
                        {
                            ["deviceType"] = deviceType,
                            ["connectionState"] = false,
                            ["deviceName"] = $"IoT-{deviceType}",
                            ["deviceState"] = false
                        }
                    }
            };
            await _registryManager.UpdateTwinAsync(deviceId, twin, "*");

            return new ResponseResult { Succeeded = true, Message = $"Device {deviceId} added successfully with device type {deviceType}." };
        }
        catch (DeviceAlreadyExistsException)
        {
            return new ResponseResult { Succeeded = false, Message = $"Device {deviceId} already exists." };
        }
        catch (Exception ex)
        {
            return new ResponseResult { Succeeded = false, Message = $"Error adding device: {ex.Message}" };
        }
    }

    public async Task<ResponseResult> RemoveDeviceAsync(string deviceId, string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return new ResponseResult { Succeeded = false, Message = "Email address cannot be empty." };
        }

        try
        {
            await _registryManager.RemoveDeviceAsync(deviceId);

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
