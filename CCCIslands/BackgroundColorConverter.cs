using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CCCIslands;

internal class BackgroundColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString() switch
        {
            "L" => new SolidColorBrush(Colors.LightGoldenrodYellow),
            "W" => new SolidColorBrush(Colors.DeepSkyBlue),
            _ => new SolidColorBrush(Colors.White)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
