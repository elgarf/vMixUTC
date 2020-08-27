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
    [ValueConversion(typeof(double[]), typeof(Thickness))]
    public class DoubleToThicknessConverter : MarkupExtension, IMultiValueConverter
    {
        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new DoubleToThicknessConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(values.Length > 0 ? (double)values[0] : 0,
                values.Length > 1 ? (double)values[1] : 0,
                values.Length > 2 ? (double)values[2] : 0,
                values.Length > 3 ? (double)values[3] : 0);
            //throw new NotImplementedException();
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
