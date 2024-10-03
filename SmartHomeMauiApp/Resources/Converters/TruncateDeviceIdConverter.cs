using System.Globalization;

namespace SmartHomeMauiApp.Resources.Converters;

public class TruncateDeviceIdConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        if (value is string deviceId && deviceId.Length > 12)
        {
            return $"{deviceId.Substring(0, 8)}...{deviceId.Substring(deviceId.Length - 4)}";
        }
        return value ?? string.Empty;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
