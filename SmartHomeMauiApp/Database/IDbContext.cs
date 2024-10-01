using Shared.Library.Models;

namespace SmartHomeMauiApp.Database;

public interface IDbContext
{
    Task<UserSettings> GetUserSettingsAsync();
    Task<int> SaveUserSettingsAsync(UserSettings userSettings);
    Task<IoTHubSettings> GetIoTHubSettingsAsync();
    Task<int> SaveIoTHubSettingsAsync(IoTHubSettings ioTHubSettings);
    Task<DeviceSettings> GetDeviceSettingsAsync(string deviceId);
    Task<List<DeviceSettings>> GetAllDeviceSettingsAsync();
    Task<int> SaveDeviceSettingsAsync(DeviceSettings deviceSettings);
    Task SeedDataAsync(string defaultEmail, string defaultConnectionString);
}
