using System;
using System.Globalization;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Maui.Controls;

namespace SmartHomeMauiApp.Resources.Converters
{
    public class ConnectionStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Twin twin)
            {
                if (twin.Properties.Reported.Contains("connectionState"))
                {
                    bool isConnected = (bool)twin.Properties.Reported["connectionState"];
                    return isConnected ? Colors.Green : Colors.Red;
                }
            }

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
