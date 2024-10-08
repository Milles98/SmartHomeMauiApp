using Microsoft.Azure.Devices.Shared;
using Moq;
using Shared.Library.Models;
using Shared.Library.Models.WeatherApi;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using SmartHomeMauiApp.Services;
using Xunit;

namespace SmartHomeMauiApp.Tests.MVVM.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IDeviceManager> _deviceManagerMock;
    private readonly Mock<ISmarthomeContext> _dbContextMock;
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly MainViewModel _mainViewModel;

    public MainViewModelTests()
    {
        _deviceManagerMock = new Mock<IDeviceManager>();
        _dbContextMock = new Mock<ISmarthomeContext>();
        _weatherServiceMock = new Mock<IWeatherService>();
        _navigationServiceMock = new Mock<INavigationService>();

        _mainViewModel = new MainViewModel(
            _deviceManagerMock.Object,
            _dbContextMock.Object,
            _weatherServiceMock.Object,
            _navigationServiceMock.Object
        );
    }

    [Fact]
    public async Task LoadDevicesAsync_ShouldPopulateDevices_WhenResponseIsSuccessful()
    {
        // Arrange:
        var devices = new List<Twin>
        {
            new Twin("device1"),
            new Twin("device2")
        };

        var responseResult = new ResponseResult
        {
            Succeeded = true,
            Content = devices
        };

        _deviceManagerMock
            .Setup(dm => dm.GetDevicesAsync(It.IsAny<string>()))
            .ReturnsAsync(responseResult);

        // Act:
        await _mainViewModel.LoadDevicesAsync();

        // Assert:
        Assert.NotEmpty(_mainViewModel.Devices);
        Assert.Equal(2, _mainViewModel.Devices.Count);
    }

    [Fact]
    public async Task LoadWeatherDataAsync_ShouldSetWeatherProperties_WhenResponseIsSuccessful()
    {
        // Arrange:
        var weatherData = new WeatherResponse
        {
            Current = new CurrentWeather
            {
                TempC = 25.0f,
                Condition = new Shared.Library.Models.WeatherApi.Condition
                {
                    Text = "Sunny",
                    Icon = "//cdn.weatherapi.com/weather/64x64/day/113.png"
                }
            }
        };

        _weatherServiceMock
            .Setup(ws => ws.GetWeatherAsync(It.IsAny<string>()))
            .ReturnsAsync(weatherData);

        // Act:
        await _mainViewModel.LoadWeatherDataAsync();

        // Assert:
        Assert.Equal("Sunny", _mainViewModel.ConditionText);
        Assert.Equal("25°C", _mainViewModel.Temperature);
        Assert.Equal("https://cdn.weatherapi.com/weather/64x64/day/113.png", _mainViewModel.WeatherIcon);
    }

    [Fact]
    public async Task NavigateToDeviceDetailAsync_ShouldNavigateToDetailPage_WhenDeviceIsNotNull()
    {
        // Arrange
        var twin = new Twin("device1");

        var userSettings = new UserSettings { EmailAddress = "test@example.com" };
        _dbContextMock.Setup(db => db.GetUserSettingsAsync())
                      .ReturnsAsync(userSettings);

        // Act
        await _mainViewModel.NavigateToDeviceDetailAsync(twin);

        // Assert:
        _navigationServiceMock.Verify(nav => nav.NavigateToAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task NavigateToDeviceDetailAsync_ShouldNotNavigate_WhenDeviceIsNull()
    {
        // Act
        await _mainViewModel.NavigateToDeviceDetailAsync(null!);

        // Assert:
        _navigationServiceMock.Verify(nav => nav.NavigateToAsync(It.IsAny<string>()), Times.Never);
    }
}
