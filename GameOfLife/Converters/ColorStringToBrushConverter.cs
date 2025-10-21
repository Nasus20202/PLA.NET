using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameOfLife.Converters;

/// <summary>
/// Converts a hex color string to a Brush
/// </summary>
public class ColorStringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString)
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFrom(colorString)!;
            }
            catch
            {
                return Brushes.LimeGreen;
            }
        }
        return Brushes.LimeGreen;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color.ToString();
        }
        return "#FF00FF00";
    }
}
