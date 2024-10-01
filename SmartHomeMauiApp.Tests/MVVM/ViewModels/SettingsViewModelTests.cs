using Moq;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.Services;
using Xunit;

namespace SmartHomeMauiApp.Tests.MVVM.ViewModels;

public class SettingsViewModelTests
{
    private readonly Mock<IDeviceManager> _deviceManagerMock;
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly Mock<IPreferencesService> _preferencesServiceMock;
    private readonly SettingsViewModel _settingsViewModel;

    public SettingsViewModelTests()
    {
        _deviceManagerMock = new Mock<IDeviceManager>();
        _dbContextMock = new Mock<IDbContext>();
        _preferencesServiceMock = new Mock<IPreferencesService>();

        _settingsViewModel = new SettingsViewModel(
            _deviceManagerMock.Object,
            _dbContextMock.Object,
            _preferencesServiceMock.Object
        );
    }

    [Fact]
    public async Task LoadSettingsAsync_ShouldPopulateSettings_WhenDataExists()
    {
        // Arrange
        var userSettings = new UserSettings { EmailAddress = "test@example.com" };
        var iotHubSettings = new IoTHubSettings { ConnectionString = "TestConnectionString" };

        _dbContextMock.Setup(db => db.GetUserSettingsAsync())
                      .ReturnsAsync(userSettings);
        _dbContextMock.Setup(db => db.GetIoTHubSettingsAsync())
                      .ReturnsAsync(iotHubSettings);

        // Act
        await _settingsViewModel.LoadSettingsAsync();

        // Assert
        Assert.Equal("test@example.com", _settingsViewModel.EmailAddress);
        Assert.Equal("TestConnectionString", _settingsViewModel.ConnectionString);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldSaveSettings_WhenConnectionStringIsValid()
    {
        // Arrange
        _settingsViewModel.ConnectionString = "ValidConnectionString";
        _settingsViewModel.EmailAddress = "test@example.com";

        _dbContextMock.Setup(db => db.SaveUserSettingsAsync(It.IsAny<UserSettings>())).ReturnsAsync(1);
        _dbContextMock.Setup(db => db.SaveIoTHubSettingsAsync(It.IsAny<IoTHubSettings>())).ReturnsAsync(1);

        // Act
        await _settingsViewModel.SaveSettingsAsync();

        // Assert
        _deviceManagerMock.Verify(dm => dm.UpdateConnectionString("ValidConnectionString"), Times.Once);
        _dbContextMock.Verify(db => db.SaveUserSettingsAsync(It.IsAny<UserSettings>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveIoTHubSettingsAsync(It.IsAny<IoTHubSettings>()), Times.Once);

        _preferencesServiceMock.Verify(ps => ps.Set("EmailAddress", "test@example.com"), Times.Once);
    }


    [Fact]
    public async Task SaveSettingsAsync_ShouldShowError_WhenConnectionStringIsEmpty()
    {
        // Arrange
        _settingsViewModel.ConnectionString = string.Empty;

        // Act
        await _settingsViewModel.SaveSettingsAsync();

        // Assert
        Assert.Equal("Connection String cannot be empty.", _settingsViewModel.ResponseMessage);
        _deviceManagerMock.Verify(dm => dm.UpdateConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldHandleException()
    {
        // Arrange
        _settingsViewModel.ConnectionString = "ValidConnectionString";
        _dbContextMock.Setup(db => db.SaveUserSettingsAsync(It.IsAny<UserSettings>())).ThrowsAsync(new Exception("Database error"));

        // Act
        await _settingsViewModel.SaveSettingsAsync();

        // Assert
        Assert.Equal("An error occurred while saving settings. Please try again.", _settingsViewModel.ResponseMessage);
        Assert.Equal("Red", _settingsViewModel.ResponseMessageColor);
    }
}
