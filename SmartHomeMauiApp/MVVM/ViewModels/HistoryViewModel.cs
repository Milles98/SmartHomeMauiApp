using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Models;
using SmartHomeMauiApp.Database;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly DbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<DeviceSettings> _deviceSettings = new();

    [ObservableProperty]
    private ObservableCollection<IoTHubSettings> _ioTHubSettings = new();

    [ObservableProperty]
    private ObservableCollection<UserSettings> _userSettings = new();

    [ObservableProperty]
    private string _responseMessage;

    public HistoryViewModel(DbContext dbContext)
    {
        _dbContext = dbContext;
        LoadSettingsAsync();
    }

    private async void LoadSettingsAsync()
    {
        try
        {
            var deviceSettingsList = await _dbContext.GetAllDeviceSettingsAsync();
            DeviceSettings = new ObservableCollection<DeviceSettings>(deviceSettingsList);

            var ioTHubSettings = await _dbContext.GetIoTHubSettingsAsync();
            IoTHubSettings = new ObservableCollection<IoTHubSettings> { ioTHubSettings };

            var userSettings = await _dbContext.GetUserSettingsAsync();
            UserSettings = new ObservableCollection<UserSettings> { userSettings };
        }
        catch (Exception ex)
        {
            ResponseMessage = "Failed to load settings.";
            Debug.WriteLine($"Error in LoadSettingsAsync: {ex.Message}");
        }
    }
}
