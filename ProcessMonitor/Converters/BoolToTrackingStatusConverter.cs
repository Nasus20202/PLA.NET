using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessMonitor.Converters;

public class BoolToTrackingStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTracked)
        {
            return isTracked ? "Stop Tracking" : "Start Tracking";
        }
        return "Start Tracking";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
