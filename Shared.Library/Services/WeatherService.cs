using Newtonsoft.Json;
using Shared.Library.Models.WeatherApi;

namespace Shared.Library.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "abe8320defb74b72adf111532242504";
    private const string BaseUrl = "http://api.weatherapi.com/v1/current.json";

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherResponse?> GetWeatherAsync(string location)
    {
        try
        {
            var url = $"{BaseUrl}?key={ApiKey}&q={location}&aqi=no";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherResponse>(response);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
