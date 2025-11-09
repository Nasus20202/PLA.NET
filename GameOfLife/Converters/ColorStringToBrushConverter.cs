using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameOfLife.Converters;

public class ColorStringToBrushConverter : IValueConverter
{
    private static readonly Brush DefaultBrush = Brushes.LimeGreen;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string colorString || string.IsNullOrWhiteSpace(colorString))
            return DefaultBrush;

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
        throw new NotImplementedException();
    }
}
