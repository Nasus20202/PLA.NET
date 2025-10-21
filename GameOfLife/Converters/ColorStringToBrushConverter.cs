using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameOfLife.Converters;

/// <summary>
/// Converts a hex color string to a Brush
/// </summary>
public class ColorStringToBrushConverter : IValueConverter
{
    private static readonly Brush DefaultBrush = Brushes.LimeGreen;
    private const string DefaultColorString = "#FF00FF00";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string colorString || string.IsNullOrWhiteSpace(colorString))
        {
            return DefaultBrush;
        }

        try
        {
            var converter = new BrushConverter();
            var result = converter.ConvertFromString(colorString);
            return result as Brush ?? DefaultBrush;
        }
        catch
        {
            return DefaultBrush;
        }
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color.ToString();
        }
        return DefaultColorString;
    }
}
