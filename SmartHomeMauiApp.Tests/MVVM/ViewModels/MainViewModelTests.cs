using Microsoft.Azure.Devices.Shared;
using Moq;
using Shared.Library.Models;
using Shared.Library.Services;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using Xunit;

namespace SmartHomeMauiApp.Tests.MVVM.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IDeviceManager> _deviceManagerMock;
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly MainViewModel _mainViewModel;

    public MainViewModelTests()
    {
        _deviceManagerMock = new Mock<IDeviceManager>();
        _dbContextMock = new Mock<IDbContext>();
        _httpClientMock = new Mock<HttpClient>();

        // Initialize the MainViewModel with mocked dependencies
        _mainViewModel = new MainViewModel(_deviceManagerMock.Object, _dbContextMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public async Task LoadDevicesAsync_ShouldPopulateDevices_WhenResponseIsSuccessful()
    {
        // Arrange: Prepare the mock response
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

        // Act: Call the method being tested
        await _mainViewModel.LoadDevicesAsync();

        // Assert: Verify that the Devices collection was populated
        Assert.NotEmpty(_mainViewModel.Devices);
        Assert.Equal(2, _mainViewModel.Devices.Count);
    }

    //[Fact]
    //public async Task LoadDevicesAsync_ShouldSetResponseMessage_WhenResponseFails()
    //{
    //    // Arrange: Set up a failure response from the device manager
    //    var responseResult = new ResponseResult
    //    {
    //        Succeeded = false,
    //        Message = "Failed to retrieve devices"
    //    };

    //    _deviceManagerMock
    //        .Setup(dm => dm.GetDevicesAsync(It.IsAny<string>()))
    //        .ReturnsAsync(responseResult);

    //    // Act: Call the method being tested
    //    await _mainViewModel.LoadDevicesAsync();

    //    // Assert: Check if the response message is set correctly
    //    Assert.Equal("Failed to retrieve devices.", _mainViewModel.ResponseMessage);
    //}

    //[Fact]
    //public async Task LoadWeatherDataAsync_ShouldSetWeatherProperties_WhenResponseIsSuccessful()
    //{
    //    // Arrange: Prepare a mock HTTP response for weather data
    //    var weatherDataJson = @"
    //    {
    //        'current': {
    //            'condition': {
    //                'text': 'Sunny',
    //                'icon': '//cdn.weatherapi.com/weather/64x64/day/113.png'
    //            },
    //            'temp_c': 25.0
    //        }
    //    }";

    //    var handler = new Mock<HttpMessageHandler>();
    //    handler.SetupAnyRequest()
    //        .ReturnsResponse(weatherDataJson, "application/json");

    //    var client = new HttpClient(handler.Object);

    //    // Replace HttpClient with the mock client in your service if needed
    //    // For example, inject the mock HttpClient into your MainViewModel

    //    // Act: Call the method being tested
    //    await _mainViewModel.LoadWeatherDataAsync();

    //    // Assert: Verify that the weather properties are set
    //    Assert.Equal("Sunny", _mainViewModel.ConditionText);
    //    Assert.Equal("25°C", _mainViewModel.Temperature);
    //    Assert.Equal("https://cdn.weatherapi.com/weather/64x64/day/113.png", _mainViewModel.WeatherIcon);
    //}
}
