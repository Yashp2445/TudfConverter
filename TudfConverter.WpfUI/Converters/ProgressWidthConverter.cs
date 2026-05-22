using System;
using System.Globalization;
using System.Windows.Data;

namespace TudfConverter.WpfUI.Converters;

public class ProgressWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[1] is double totalWidth && totalWidth > 0)
        {
            double progress = 0;
            if (values[0] is int pInt) progress = pInt;
            else if (values[0] is double pDouble) progress = pDouble;

            progress = Math.Max(0, Math.Min(100, progress));
            return (progress / 100.0) * totalWidth;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
