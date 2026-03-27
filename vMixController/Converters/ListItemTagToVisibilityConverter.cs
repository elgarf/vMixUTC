using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace vMixController.Converters
{
    public class ListItemTagToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ListItemTagToVisibilityConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as string;
            if (string.IsNullOrEmpty(val))
            {
                return Visibility.Collapsed;
            }

            var escaped = Helpers.EscapeAt(val);
            if (string.IsNullOrEmpty(escaped) || string.IsNullOrEmpty(Helpers.EscapeSymbol))
            {
                return Visibility.Collapsed;
            }

            var ts = escaped.Split(Helpers.EscapeSymbol[0]);
            var tag = ts.Length >= 2 ? Helpers.UnescapeAt(ts[1]) : null;
            return string.IsNullOrWhiteSpace(tag) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
