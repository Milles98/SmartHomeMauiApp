using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Library.Models.IotResources;
using Shared.Library.Services;

namespace AzureFunctions.Functions;

public class DeviceRegistration
{
    private readonly ILogger<DeviceRegistration> _logger;
    private readonly IDeviceManager _deviceManager;

    public DeviceRegistration(ILogger<DeviceRegistration> logger, IDeviceManager deviceManager)
    {
        _logger = logger;
        _deviceManager = deviceManager;
    }

    [Function("DeviceRegistration")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Received a device registration request.");

        var body = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation("Request body read successfully.");

        var drr = JsonConvert.DeserializeObject<DeviceRegistrationRequest>(body);

        if (drr == null || string.IsNullOrEmpty(drr.DeviceId) || string.IsNullOrEmpty(drr.DeviceName))
        {
            _logger.LogWarning("Invalid request: Missing 'deviceId' or 'deviceName'.");
            return new BadRequestObjectResult("Invalid request body, 'deviceId' or 'deviceName' is missing");
        }

        _logger.LogInformation($"Registering device with ID: {drr.DeviceId}, Name: {drr.DeviceName}");

        try
        {
            var result = await _deviceManager.RegisterDeviceAsync(drr.DeviceId, drr.DeviceName, drr.DeviceType);
            _logger.LogInformation($"Device {drr.DeviceId} registered successfully.");

            string? deviceName = null;
            string? deviceType = null;

            if (result.Twin?.Properties.Desired.Contains("deviceName") == true)
            {
                deviceName = result.Twin.Properties.Desired["deviceName"].ToString();
            }
            else
            {
                _logger.LogWarning("Device name is missing in Twin desired properties.");
            }

            if (result.Twin?.Properties.Desired.Contains("deviceType") == true)
            {
                deviceType = result.Twin.Properties.Desired["deviceType"].ToString();
            }
            else
            {
                _logger.LogWarning("Device type is missing in Twin desired properties.");
            }

            var response = new DeviceRegistrationResponse
            {
                DeviceId = result.Device?.Id ?? "Unknown DeviceId",
                ConnectionString = result.ConnectionString ?? "No Connection String",
                DeviceName = deviceName ?? "Unknown DeviceName",
                DeviceType = deviceType ?? "Unknown DeviceType"
            };

            _logger.LogInformation($"Returning registration details for device {drr.DeviceId}.");
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while registering device {drr.DeviceId}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
