using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using vMixController.Classes;

namespace vMixController.Converters
{
    [ValueConversion(typeof(byte), typeof(Visibility))]
    public class BitToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new BitToVisibilityConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte.TryParse((string)parameter, out byte bit);
            return ((byte)value).GetBit(bit) ? Visibility.Visible : Visibility.Collapsed;
            
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
