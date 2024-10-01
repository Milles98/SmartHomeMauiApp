using Moq;
using Moq.Protected;
using Shared.Library.Services;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.Services;
using System.Net;
using Xunit;

namespace SmartHomeMauiApp.Tests.MVVM.ViewModels;

public class AddDeviceViewModelTests
{
    private readonly Mock<IDeviceManager> _deviceManagerMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly AddDeviceViewModel _viewModel;

    public AddDeviceViewModelTests()
    {
        _deviceManagerMock = new Mock<IDeviceManager>();
        _navigationServiceMock = new Mock<INavigationService>();

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        _viewModel = new AddDeviceViewModel(
            _deviceManagerMock.Object,
            _navigationServiceMock.Object,
            _httpClient
        );
    }

    [Fact]
    public void GenerateDeviceId_ShouldSetDeviceId()
    {
        // Act
        _viewModel.GenerateDeviceIdCommand.Execute(null);

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.DeviceId));
    }

    [Fact]
    public async Task AddDeviceAsync_ShouldSetErrorMessage_WhenDeviceIdOrNameIsEmpty()
    {
        // Arrange
        _viewModel.DeviceId = string.Empty;
        _viewModel.DeviceName = string.Empty;
        _viewModel.SelectedDeviceType = "Lamp";

        // Act
        await _viewModel.AddDeviceAsync();

        // Assert
        Assert.Equal("Please enter device name, select device type, and generate id", _viewModel.ResponseMessage);
    }

    [Fact]
    public async Task AddDeviceAsync_ShouldAddDeviceSuccessfully()
    {
        // Arrange
        var deviceId = "test-device-id";
        var deviceName = "Test Device";
        var deviceType = "Lamp";

        _viewModel.DeviceId = deviceId;
        _viewModel.DeviceName = deviceName;
        _viewModel.SelectedDeviceType = deviceType;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":\"success\"}")
            });

        // Act
        await _viewModel.AddDeviceAsync();

        // Assert
        Assert.Equal("Device added successfully!", _viewModel.ResponseMessage);
        Assert.Equal("Green", _viewModel.ResponseMessageColor);
        _navigationServiceMock.Verify(nav => nav.NavigateToAsync("///MainPage"), Times.Once);
    }

    [Fact]
    public async Task AddDeviceAsync_ShouldSetErrorMessage_WhenDeviceRegistrationFails()
    {
        // Arrange
        _viewModel.DeviceId = "test-device-id";
        _viewModel.DeviceName = "Test Device";
        _viewModel.SelectedDeviceType = "Lamp";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        await _viewModel.AddDeviceAsync();

        // Assert
        Assert.StartsWith("Failed to register device", _viewModel.ResponseMessage);
        Assert.Equal("Red", _viewModel.ResponseMessageColor);
        _deviceManagerMock.Verify(dm => dm.GetDevicesAsync(It.IsAny<string>()), Times.Never);
        _navigationServiceMock.Verify(nav => nav.NavigateToAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddDeviceAsync_ShouldHandleException()
    {
        // Arrange
        _viewModel.DeviceId = "test-device-id";
        _viewModel.DeviceName = "Test Device";
        _viewModel.SelectedDeviceType = "Lamp";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Something went wrong"));

        // Act
        await _viewModel.AddDeviceAsync();

        // Assert
        Assert.StartsWith("An error occurred:", _viewModel.ResponseMessage);
        Assert.Equal("Red", _viewModel.ResponseMessageColor);
    }
}
