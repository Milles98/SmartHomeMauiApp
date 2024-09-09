using System.Globalization;

namespace SmartHomeMauiApp.Resources.Converters;

public class BooleanToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool isConnected)
		{
			return isConnected ? Colors.Green : Colors.Red;
		}

		return Colors.Gray; // Default färg om värdet inte är bool
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
