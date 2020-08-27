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
    [ValueConversion(typeof(double), typeof(double))]
    class LogarithmicConverter : MarkupExtension, IValueConverter
    {

        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new LogarithmicConverter());

        private static double min = 1e-32f;
        private Func<double, double> _in = (x) =>  20 * Math.Log10(x);
        private Func<double, double> _out = (x) => Math.Pow(10, x / 20);

        public LogarithmicConverter()
        {
            _in = (x) => Math.Pow(x, 1d/4);
            _out = (x) => Math.Pow(x, 4);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (double)value / 100;
            if (v < min)
                v = min;
            return _in(v) * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (double)value / 100;
            return _out(v) * 100;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
