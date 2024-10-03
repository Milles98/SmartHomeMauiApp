using Microsoft.Azure.Devices.Shared;
using System.Globalization;

namespace SmartHomeMauiApp.Resources.Converters;

public class TwinDeviceNameConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        var twin = value as Twin;
        if (twin != null)
        {
            if (twin.Properties.Desired.Contains("deviceName"))
            {
                return twin.Properties.Desired["deviceName"].ToString();
            }
            else if (twin.Properties.Reported.Contains("deviceName"))
            {
                return twin.Properties.Reported["deviceName"].ToString();
            }
        }
        return "Unknown Device";
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
