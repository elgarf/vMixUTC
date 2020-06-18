using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class IntegersToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = true;
            bool areInts = true;
            foreach (var item in values)
                areInts = areInts && item is int;
            if (!areInts)
                return Visibility.Visible;
            for (int i = 0; i < values.Length - 1; i++)
                result = result && ((int)values[i] == (int)values[i + 1]);
            return result ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return Array.Empty<object>();
            //throw new NotImplementedException();
        }
    }
}
