using Shared.Library.Models.WeatherApi;

namespace Shared.Library.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherAsync(string location);
}
