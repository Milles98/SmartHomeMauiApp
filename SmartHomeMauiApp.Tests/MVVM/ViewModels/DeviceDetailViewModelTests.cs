using Microsoft.Azure.Devices;
using Moq;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.Services;
using Xunit;

namespace SmartHomeMauiApp.Tests.MVVM.ViewModels;

public class DeviceDetailViewModelTests
{
    private readonly Mock<IDeviceManager> _deviceManagerMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ISmarthomeContext> _dbContextMock;
    private readonly Mock<ITimerService> _timerServiceMock;
    private DeviceDetailViewModel _viewModel;

    public DeviceDetailViewModelTests()
    {
        _deviceManagerMock = new Mock<IDeviceManager>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dbContextMock = new Mock<ISmarthomeContext>();
        _timerServiceMock = new Mock<ITimerService>();

        _viewModel = new DeviceDetailViewModel(
            _deviceManagerMock.Object,
            _dbContextMock.Object,
            _navigationServiceMock.Object,
            _timerServiceMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_ShouldLoadUserSettingsAndStartTimer()
    {
        // Arrange
        var userSettings = new UserSettings { EmailAddress = "test@example.com" };
        _dbContextMock.Setup(db => db.GetUserSettingsAsync()).ReturnsAsync(userSettings);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        _dbContextMock.Verify(db => db.GetUserSettingsAsync(), Times.Once);
        _timerServiceMock.Verify(ts => ts.Start(It.IsAny<Action>(), 5000), Times.Once);
        Assert.Equal("test@example.com", _viewModel.EmailAddress);
    }

    [Fact]
    public async Task ToggleStateAsync_ShouldShowError_WhenConnectionStateIsInvalid()
    {
        // Arrange
        _viewModel.ConnectionState = "false";

        // Act
        await _viewModel.ToggleStateAsync();

        // Assert
        _navigationServiceMock.Verify(ns => ns.ShowAlertAsync("Error", "Failed to toggle device state. Ensure the device connection is correct and the app is running.", "Ok"), Times.Once);
    }

    [Fact]
    public async Task RemoveDeviceAsync_ShouldShowError_WhenEmailAddressIsEmpty()
    {
        // Arrange
        _viewModel.EmailAddress = string.Empty;

        // Act
        await _viewModel.RemoveDeviceAsync();

        // Assert
        _navigationServiceMock.Verify(ns => ns.ShowAlertAsync("Error", "No email address registered. Cannot remove device.", "Ok"), Times.Once);
    }

    [Fact]
    public async Task RemoveDeviceAsync_ShouldSendEmailAndRemoveDevice_WhenConfirmed()
    {
        // Arrange
        _viewModel.EmailAddress = "test@example.com";
        _navigationServiceMock.Setup(ns => ns.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var response = new ResponseResult { Succeeded = true };
        _deviceManagerMock.Setup(dm => dm.DeviceRemovalSendEmailAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response);

        // Act
        await _viewModel.RemoveDeviceAsync();

        // Assert
        _deviceManagerMock.Verify(dm => dm.DeviceRemovalSendEmailAsync(_viewModel.DeviceId!, _viewModel.EmailAddress), Times.Once);
        _navigationServiceMock.Verify(ns => ns.NavigateToAsync("///MainPage"), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldStopTimer_WhenDisposed()
    {
        // Act
        _viewModel.Dispose();

        // Assert
        _timerServiceMock.Verify(ts => ts.Stop(), Times.Once);
    }

    [Fact]
    public async Task ToggleStateAsync_ShouldInvokeDirectMethod_WhenConnectionStateIsValid()
    {
        // Arrange
        _viewModel.ConnectionState = "true";
        _viewModel.DeviceState = "On";
        var response = new ResponseResult { Succeeded = true, Content = new CloudToDeviceMethodResult { Status = 200 } };

        _deviceManagerMock.Setup(dm => dm.InvokeDirectMethodAsync(It.IsAny<string>(), It.IsAny<string>(), null, 30))
                          .ReturnsAsync(response);

        // Act
        await _viewModel.ToggleStateAsync();

        // Assert
        _deviceManagerMock.Verify(dm => dm.InvokeDirectMethodAsync(_viewModel.DeviceId!, "stop", null, 30), Times.Once);
        _navigationServiceMock.Verify(ns => ns.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConnectAsync_ShouldInvokeConnectDirectMethod_WhenCalled()
    {
        // Arrange
        var response = new ResponseResult { Succeeded = true, Content = new CloudToDeviceMethodResult { Status = 200 } };

        _deviceManagerMock.Setup(dm => dm.InvokeDirectMethodAsync(It.IsAny<string>(), "connect", null, 30))
                          .ReturnsAsync(response);

        // Act
        await _viewModel.ConnectAsync();

        // Assert
        _deviceManagerMock.Verify(dm => dm.InvokeDirectMethodAsync(_viewModel.DeviceId!, "connect", null, 30), Times.Once);
    }
}
