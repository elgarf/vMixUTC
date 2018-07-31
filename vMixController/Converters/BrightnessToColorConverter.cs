using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace vMixController.Converters
{
    public class BrightnessToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var clr = ((Color)value);
            var Y = 0.2126 * (clr.R / 255f) + 0.7152 * (clr.G / 255f) + 0.0722 * (clr.B / 255f);
            if (Y >= 0.5f)
                return Brushes.Black;
            else
                return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
