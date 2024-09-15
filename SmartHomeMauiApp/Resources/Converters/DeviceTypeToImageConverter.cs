using Microsoft.Azure.Devices.Shared;
using System.Globalization;

namespace SmartHomeMauiApp.Resources.Converters;

public class DeviceTypeToImageConverter : IValueConverter
{
    private static readonly Dictionary<string, string> DeviceTypeToImageMap = new Dictionary<string, string>
    {
        { "Fan", "fan_icon.png" },
        { "Lamp", "maui_lamp.png" },
        { "Ac", "ac_icon.png" },
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Twin twin && twin.Properties.Reported.Contains("deviceType"))
        {
            string deviceType = twin.Properties.Reported["deviceType"].ToString();
            return DeviceTypeToImageMap.TryGetValue(deviceType, out var imageName) ? imageName : "microchip.png";
        }

        return "microchip.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
