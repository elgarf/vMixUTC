using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace vMixController.Converters
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class AudiobusToBoolConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new AudiobusToBoolConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = value as string;
            var needle = parameter as string;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(needle))
            {
                return false;
            }

            return source.IndexOf(needle, StringComparison.Ordinal) >= 0;
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
