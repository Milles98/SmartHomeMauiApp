namespace Shared.Library.Models.WeatherApi;

public class WeatherResponse
{
    public Location? Location { get; set; }
    public CurrentWeather? Current { get; set; }
}
