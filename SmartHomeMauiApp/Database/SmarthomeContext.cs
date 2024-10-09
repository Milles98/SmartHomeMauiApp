using Shared.Library.Models;
using SQLite;
using System.Diagnostics;

namespace SmartHomeMauiApp.Database;

public class SmarthomeContext : ISmarthomeContext
{
    private readonly SQLiteAsyncConnection _database;

    public SmarthomeContext()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smarthome.db3");
        Debug.WriteLine($"Db added to {dbPath}");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _database.CreateTableAsync<UserSettings>();
            await _database.CreateTableAsync<IoTHubSettings>();
            await _database.CreateTableAsync<DeviceSettings>();
            Debug.WriteLine("Database tables created successfully.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing database: {ex.Message}");
        }
    }

    public async Task<UserSettings> GetUserSettingsAsync()
    {
        try
        {
            var userSettings = await _database.Table<UserSettings>().FirstOrDefaultAsync();
            Debug.WriteLine("User settings retrieved successfully.");
            return userSettings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting user settings: {ex.Message}");
            return null!;
        }
    }

    public async Task<int> SaveUserSettingsAsync(UserSettings userSettings)
    {
        try
        {
            await _database.RunInTransactionAsync(tran =>
            {
                tran.DeleteAll<UserSettings>();
                tran.Insert(userSettings);
            });
            Debug.WriteLine("User settings saved successfully.");
            return 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving user settings: {ex.Message}");
            return 0;
        }
    }

    public async Task<IoTHubSettings> GetIoTHubSettingsAsync()
    {
        try
        {
            var iotHubSettings = await _database.Table<IoTHubSettings>().FirstOrDefaultAsync();
            Debug.WriteLine("IoT Hub settings retrieved successfully.");
            return iotHubSettings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting IoT Hub settings: {ex.Message}");
            return null!;
        }
    }

    public async Task<int> SaveIoTHubSettingsAsync(IoTHubSettings ioTHubSettings)
    {
        try
        {
            var existingIoTHubSettings = await _database.Table<IoTHubSettings>().FirstOrDefaultAsync();
            if (existingIoTHubSettings != null)
            {
                ioTHubSettings.Id = existingIoTHubSettings.Id;
                var result = await _database.UpdateAsync(ioTHubSettings);
                Debug.WriteLine("IoT Hub settings updated successfully.");
                return result;
            }
            var insertResult = await _database.InsertAsync(ioTHubSettings);
            Debug.WriteLine("IoT Hub settings inserted successfully.");
            return insertResult;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving IoT Hub settings: {ex.Message}");
            return 0;
        }
    }

    public async Task<DeviceSettings> GetDeviceSettingsAsync(string deviceId)
    {
        try
        {
            var deviceSettings = await _database.Table<DeviceSettings>().Where(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
            Debug.WriteLine("Device settings retrieved successfully.");
            return deviceSettings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting device settings: {ex.Message}");
            return null!;
        }
    }

    public async Task<List<DeviceSettings>> GetAllDeviceSettingsAsync()
    {
        try
        {
            var deviceSettingsList = await _database.Table<DeviceSettings>().ToListAsync();
            Debug.WriteLine("All device settings retrieved successfully.");
            return deviceSettingsList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting all device settings: {ex.Message}");
            return new List<DeviceSettings>();
        }
    }

    public async Task<int> SaveDeviceSettingsAsync(DeviceSettings deviceSettings)
    {
        try
        {
            var existingDevice = await _database.Table<DeviceSettings>()
                .Where(d => d.DeviceId == deviceSettings.DeviceId)
                .FirstOrDefaultAsync();

            if (existingDevice != null)
            {
                deviceSettings.Id = existingDevice.Id;
                var result = await _database.UpdateAsync(deviceSettings);
                Debug.WriteLine("Device settings updated successfully.");
                return result;
            }

            var insertResult = await _database.InsertAsync(deviceSettings);
            Debug.WriteLine("Device settings inserted successfully.");
            return insertResult;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving device settings: {ex.Message}");
            return 0;
        }
    }

    public async Task SeedDataIntoDbAsync(string defaultConnectionString)
    {
        try
        {
            var iotHubSettings = await GetIoTHubSettingsAsync();
            if (iotHubSettings == null)
            {
                iotHubSettings = new IoTHubSettings { ConnectionString = defaultConnectionString };
                await SaveIoTHubSettingsAsync(iotHubSettings);
                Debug.WriteLine("Default IoT Hub settings seeded.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error seeding data: {ex.Message}");
        }
    }
}
