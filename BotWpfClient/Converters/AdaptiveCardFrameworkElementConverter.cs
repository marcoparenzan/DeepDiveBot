using AdaptiveCards;
using AdaptiveCards.Rendering.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BotWpfClient.Converters
{
    public class AdaptiveCardFrameworkElementConverter : IValueConverter
    {
        static AdaptiveCardRenderer renderer = new AdaptiveCardRenderer();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AdaptiveCard ac)
            {
                var result = renderer.RenderCard(ac);
                return result.FrameworkElement;
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}