using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using vMixController.Classes;
using vMixController.Widgets;

namespace vMixController.Converters
{
    public class ControlIntToParameterConverter : MarkupExtension, IMultiValueConverter
    {
        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new ControlIntToParameterConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null;
            }

            if (!(values[0] is vMixControl control) || !(values[1] is int index))
            {
                return null;
            }

            return new ControlIntParameter(control, index);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
