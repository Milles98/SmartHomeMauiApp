using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Shared.Library.Models.WeatherApi;

public class CurrentWeather
{
    [JsonProperty("temp_c")]
    public double TempC { get; set; }
    [JsonProperty("condition")]
    public Condition? Condition { get; set; }
}
