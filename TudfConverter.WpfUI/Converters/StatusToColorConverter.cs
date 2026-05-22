using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TudfConverter.WpfUI.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Accepted" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3DDC84")),
                "Rejected" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5370")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEFFFF"))
            };
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEFFFF"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
