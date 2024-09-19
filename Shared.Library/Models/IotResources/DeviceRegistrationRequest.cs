namespace Shared.Library.Models.IotResources;

public class DeviceRegistrationRequest
{
    public string DeviceId { get; set; } = null!;
    public string DeviceName { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
}
