using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using vMixAPI;

namespace vMixController.Converters
{
    [ValueConversion(typeof(List<Master>), typeof(Visibility))]
    public class AudiobusToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new AudiobusToVisibilityConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<Master> && parameter is string)
                foreach (var item in (List<Master>)value)
                {
                    if (item.Name == (string)parameter)
                        return Visibility.Visible;
                }
            return Visibility.Collapsed;
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
