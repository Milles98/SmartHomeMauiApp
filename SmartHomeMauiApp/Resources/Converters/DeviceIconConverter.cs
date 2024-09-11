using Microsoft.Azure.Devices.Shared;
using System.Globalization;

namespace SmartHomeMauiApp.Resources.Converters;

public class DeviceIconConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is Twin twin && twin.Properties.Reported.Contains("DeviceType"))
		{
			string deviceType = twin.Properties.Reported["DeviceType"].ToString();

			return deviceType switch
			{
				"Fan" => "fan_icon.png",
				"Lamp" => "maui_lamp.png",
				"TemperatureSensor" => "maui_temperature.png",
				_ => "dotnet_bot.png"
			};
		}

		return "dotnet_bot.png";
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
