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
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BoolToSolidColorBrushConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new BoolToSolidColorBrushConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = (bool)value;
            if (parameter != null)
                t = !t;
            if (t)
                return new SolidColorBrush(Colors.Red);
            else
                return new SolidColorBrush(Colors.SlateGray);
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
