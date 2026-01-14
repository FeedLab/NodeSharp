using System.Globalization;

namespace NodeSharp.Client.Converter;

public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "Error" => Colors.Red,
            "Warning" => Colors.Yellow,
            _ => Colors.LightGreen
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}