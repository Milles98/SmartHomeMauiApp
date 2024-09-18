using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Functions
{
    public class DeviceRegistration
    {
        private readonly ILogger<DeviceRegistration> _logger;

        public DeviceRegistration(ILogger<DeviceRegistration> logger)
        {
            _logger = logger;
        }

        [Function("DeviceRegistration")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {

            return new OkObjectResult("");
        }
    }
}
