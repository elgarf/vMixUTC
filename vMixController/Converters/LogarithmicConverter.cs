using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    class LogarithmicConverter : IValueConverter
    {
        private static double min = 1e-32f;
        private Func<double, double> _in = (x) =>  20 * Math.Log10(x);
        private Func<double, double> _out = (x) => Math.Pow(10, x / 20);

        float Map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

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

            //throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (double)value / 100;
            return _out(v) * 100;
            //return value;
            //throw new NotImplementedException();
        }
    }
}
