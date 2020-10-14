using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using vMixController.Classes;

namespace vMixController.Converters
{
    [ValueConversion(typeof(string), typeof(Color))]
    public class StatusToColorConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new StatusToColorConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Colors.Transparent;
            switch ((Status)value)
            {
                case Status.Offline:
                    return Colors.Red;
                case Status.Online:
                    return Colors.Lime;
                case Status.Sync:
                    return Colors.Yellow;
                default:
                    return Colors.Yellow;
            }
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
