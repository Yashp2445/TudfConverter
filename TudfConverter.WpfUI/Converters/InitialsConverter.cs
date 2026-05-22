using System;
using System.Globalization;
using System.Windows.Data;

namespace TudfConverter.WpfUI.Converters;

public class InitialsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value as string;
        if (string.IsNullOrWhiteSpace(str)) return "TC";
        if (str.Length >= 2) return str.Substring(0, 2).ToUpperInvariant();
        return str.ToUpperInvariant();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
