using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class MarginMultiConverter : IMultiValueConverter
    {
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
    }
}
