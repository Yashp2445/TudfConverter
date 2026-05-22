using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TudfConverter.WpfUI.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null || (value is string s && string.IsNullOrEmpty(s)) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
