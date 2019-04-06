using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using vMixController.Widgets;

namespace vMixController.Converters
{
    public class ControlToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ctrl = ((vMixControl)value);
            return parameter != null ? ctrl.Top + ctrl.Height / 2 : ctrl.Left + ctrl.Width / 2;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
