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
    private readonly DeviceManager _deviceManager;

    public DeviceRegistration(ILogger<DeviceRegistration> logger, DeviceManager deviceManager)
    {
        _logger = logger;
        _deviceManager = deviceManager;
    }

    [Function("DeviceRegistration")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var drr = JsonConvert.DeserializeObject<DeviceRegistrationRequest>(body);

        if (drr == null || string.IsNullOrEmpty(drr.DeviceId) || string.IsNullOrEmpty(drr.DeviceName))
            return new BadRequestObjectResult("Invalid request body, 'deviceId' or 'deviceName' is missing");

        var result = await _deviceManager.RegisterDeviceAsync(drr!.DeviceId, drr.DeviceName);

        return new OkObjectResult(result);
    }

    //Lägg till function för att radera en enhet också o koppla in den
}
