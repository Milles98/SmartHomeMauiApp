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
        return ioTHubSettings.Id != 0 ?
            _database.UpdateAsync(ioTHubSettings) :
            _database.InsertAsync(ioTHubSettings);
    }

    public Task<DeviceSettings> GetDeviceSettingsAsync(string deviceId)
    {
        return _database.Table<DeviceSettings>().Where(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
    }

    public Task<List<DeviceSettings>> GetAllDeviceSettingsAsync()
    {
        return _database.Table<DeviceSettings>().ToListAsync();
    }

    public Task<int> SaveDeviceSettingsAsync(DeviceSettings deviceSettings)
    {
        return deviceSettings.Id != 0 ?
            _database.UpdateAsync(deviceSettings) :
            _database.InsertAsync(deviceSettings);
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
