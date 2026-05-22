using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TudfConverter.WpfUI.Converters;

public class OutcomeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string outcome)
        {
            return outcome switch
            {
                "RejectRecord" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5370")),
                "RejectSegment" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB6B")),
                "RejectField" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB74D")),
                "Ignore" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0BEC5")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEFFFF"))
            };
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEFFFF"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
