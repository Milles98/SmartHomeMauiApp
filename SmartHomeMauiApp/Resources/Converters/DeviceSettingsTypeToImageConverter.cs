using Shared.Library.Models;
using System.Globalization;

namespace SmartHomeMauiApp.Resources.Converters;

public class DeviceSettingsTypeToImageConverter : IValueConverter
{
    private static readonly Dictionary<string, string> DeviceTypeToImageMap = new Dictionary<string, string>
    {
        { "Fan", "fan_icon.png" },
        { "Lamp", "maui_lamp.png" },
        { "Ac", "ac_icon.png" },
    };

    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        if (value is DeviceSettings deviceSettings && !string.IsNullOrEmpty(deviceSettings.DeviceType))
        {
            string deviceType = deviceSettings.DeviceType;
            return DeviceTypeToImageMap.TryGetValue(deviceType, out var imageName) ? imageName : "microchip.png";
        }

        return "microchip.png";
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
