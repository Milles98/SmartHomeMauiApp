using Shared.Library.Models;
using SQLite;

namespace SmartHomeMauiApp.Database;

public class SmarthomeContext : ISmarthomeContext
{
    private readonly SQLiteAsyncConnection _database;

    public SmarthomeContext()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smarthome.db3");
        Console.WriteLine($"Db added to {dbPath}");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<UserSettings>();
        await _database.CreateTableAsync<IoTHubSettings>();
        await _database.CreateTableAsync<DeviceSettings>();
    }

    public Task<UserSettings> GetUserSettingsAsync()
    {
        return _database.Table<UserSettings>().FirstOrDefaultAsync();
    }

    public async Task<int> SaveUserSettingsAsync(UserSettings userSettings)
    {
        await _database.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<UserSettings>();
            tran.Insert(userSettings);
        });
        return 1;
    }

    public Task<IoTHubSettings> GetIoTHubSettingsAsync()
    {
        return _database.Table<IoTHubSettings>().FirstOrDefaultAsync();
    }

    public async Task<int> SaveIoTHubSettingsAsync(IoTHubSettings ioTHubSettings)
    {
        var existingIoTHubSettings = await _database.Table<IoTHubSettings>().FirstOrDefaultAsync();
        if (existingIoTHubSettings != null)
        {
            ioTHubSettings.Id = existingIoTHubSettings.Id;
            return await _database.UpdateAsync(ioTHubSettings);
        }
        return await _database.InsertAsync(ioTHubSettings);
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
