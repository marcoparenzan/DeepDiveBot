using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BotWpfClient.Converters
{
    public class AwakeningConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
       {
            if (value is bool awakening)
            {
                return awakening ? Brushes.Red : Brushes.LightGoldenrodYellow;
            }
            return Colors.LightGoldenrodYellow;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}