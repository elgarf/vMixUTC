using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class DateTimeToDayOfWeek : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;
            switch ((string)parameter)
            {
                case "M":
                    return (date.Millisecond & 1) == 1;
                case "T":
                    return (date.Millisecond & (1 << 1)) == 1 << 1;
                case "W":
                    return (date.Millisecond & (1 << 2)) == 1 << 2;
                case "TH":
                    return (date.Millisecond & (1 << 3)) == 1 << 3;
                case "F":
                    return (date.Millisecond & (1 << 4)) == 1 << 4;
                case "S":
                    return (date.Millisecond & (1 << 5)) == 1 << 5;
                case "SU":
                    return (date.Millisecond & (1 << 6)) == 1 << 6;

            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
