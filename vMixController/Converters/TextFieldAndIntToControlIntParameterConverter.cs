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
            try
            {
                return new Classes.ControlIntParameter() { A = (vMixController.Widgets.vMixControlTextField)values[0], B = (int)values[1] };
            }
            catch (Exception)
            {
                return null;
            }
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
