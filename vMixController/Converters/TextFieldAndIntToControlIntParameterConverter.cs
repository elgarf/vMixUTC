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

    public class TextFieldAndIntToControlIntParameterConverter : MarkupExtension, IMultiValueConverter
    {

        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new TextFieldAndIntToControlIntParameterConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null;
            }

            if (!(values[0] is vMixController.Widgets.vMixControlTextField textField) || !(values[1] is int index))
            {
                return null;
            }

            return new Classes.ControlIntParameter() { A = textField, B = index };
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
