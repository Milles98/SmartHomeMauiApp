using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace Shared.Library.Models.IotResources;

public class DeviceInstance
{
    public string? ConnectionString { get; set; }
    public Device? Device { get; set; }
    public Twin? Twin { get; set; }
}
