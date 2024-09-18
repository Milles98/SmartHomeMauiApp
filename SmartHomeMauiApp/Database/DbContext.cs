using Shared.Library.Models;
using SQLite;

namespace SmartHomeMauiApp.Database;

public class DbContext
{
    private readonly SQLiteAsyncConnection _database;

    public DbContext()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "smarthome.db3");

        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<UserSettings>().Wait();
        _database.CreateTableAsync<IoTHubSettings>().Wait();
        _database.CreateTableAsync<DeviceSettings>().Wait();
    }

    public Task<UserSettings> GetUserSettingsAsync()
    {
        return _database.Table<UserSettings>().FirstOrDefaultAsync();
    }

    public async Task<int> SaveUserSettingsAsync(UserSettings userSettings)
    {
        int result = 0;

        await _database.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<UserSettings>();
            result = tran.Insert(userSettings);
        });

        return result;
    }

    public Task<IoTHubSettings> GetIoTHubSettingsAsync()
    {
        return _database.Table<IoTHubSettings>().FirstOrDefaultAsync();
    }


    public Task<int> SaveIoTHubSettingsAsync(IoTHubSettings ioTHubSettings)
    {
        var existingIoTHubSettings = _database.Table<IoTHubSettings>().FirstOrDefaultAsync().Result;

        if (existingIoTHubSettings != null)
        {
            ioTHubSettings.Id = existingIoTHubSettings.Id;
            return _database.UpdateAsync(ioTHubSettings);
        }

        return _database.InsertAsync(ioTHubSettings);
    }

    public Task<DeviceSettings> GetDeviceSettingsAsync(string deviceId)
    {
        return _database.Table<DeviceSettings>().Where(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
    }

    public Task<List<DeviceSettings>> GetAllDeviceSettingsAsync()
    {
        return _database.Table<DeviceSettings>().ToListAsync();
    }

    public async Task<int> SaveDeviceSettingsAsync(DeviceSettings deviceSettings)
    {
        var existingDevice = await _database.Table<DeviceSettings>()
                                        .Where(d => d.DeviceId == deviceSettings.DeviceId)
                                        .FirstOrDefaultAsync();

        if (existingDevice != null)
        {
            deviceSettings.Id = existingDevice.Id;
            return await _database.UpdateAsync(deviceSettings);
        }

        return await _database.InsertAsync(deviceSettings);
    }

    public async Task SeedDataAsync(string defaultEmail, string defaultConnectionString)
    {
        var userSettings = await GetUserSettingsAsync();
        if (userSettings == null)
        {
            userSettings = new UserSettings { EmailAddress = defaultEmail };
            await SaveUserSettingsAsync(userSettings);
        }

        var iotHubSettings = await GetIoTHubSettingsAsync();
        if (iotHubSettings == null)
        {
            iotHubSettings = new IoTHubSettings { ConnectionString = defaultConnectionString };
            await SaveIoTHubSettingsAsync(iotHubSettings);
        }
    }
}
