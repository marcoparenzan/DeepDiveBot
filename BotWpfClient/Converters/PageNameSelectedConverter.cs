using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BotWpfClient.Converters
{
    public class PageNameSelectedConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
       {
            if (value is string pageName)
            {
                var requestedPageName = (string)parameter;
                return requestedPageName == pageName;
            }
            return Colors.LightGoldenrodYellow;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}