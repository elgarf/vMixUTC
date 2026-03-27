using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace vMixController.Converters
{
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class ColorToSolidBrushConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ColorToSolidBrushConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Color color ? new SolidColorBrush(color) : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is SolidColorBrush brush ? brush.Color : Colors.Transparent;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
