using Microsoft.Azure.Devices;
using Shared.Library.Models;
using Shared.Library.Models.IotResources;

namespace Shared.Library.Services;

public interface IDeviceManager
{
    bool Disconnect();
    Task<DeviceInstance> RegisterDeviceAsync(string deviceId, string deviceName, string deviceType);
    string GetDeviceConnectionString(Device device);
    Task<bool> UpdateDesiredPropertiesAsync(Device device, Dictionary<string, string> desiredProperties);
    void UpdateConnectionString(string connectionString);
    Task<ResponseResult> GetDevicesAsync(string query);
    Task<ResponseResult> GetDeviceTwinAsync(string deviceId);
    Task<ResponseResult> InvokeDirectMethodAsync(string deviceId, string methodName, object? payload = null, int responseTimeoutSeconds = 30);
    Task<ResponseResult> DeviceRemovalSendEmailAsync(string deviceId, string emailAddress);

}
