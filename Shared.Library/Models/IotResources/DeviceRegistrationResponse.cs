namespace Shared.Library.Models.IotResources;

public class DeviceRegistrationResponse
{
    public string? DeviceId { get; set; }
    public string? ConnectionString { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
}
