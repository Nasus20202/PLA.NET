using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessMonitor.Converters;

public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
            return string.Empty;

        return timeSpan switch
        {
            _ when timeSpan.TotalDays >= 1 =>
                $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m",
            _ when timeSpan.TotalHours >= 1 =>
                $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s",
            _ when timeSpan.TotalMinutes >= 1 => $"{timeSpan.Minutes}m {timeSpan.Seconds}s",
            _ => $"{timeSpan.Seconds}s",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
