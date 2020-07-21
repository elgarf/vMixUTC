using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class DateConverter : ValidationRule, IValueConverter
    {
        private DateTime date;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            date = ((DateTime)(value));

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return date;

            DateTime dateIn;
            DateTime.TryParse((string)value, out dateIn);

            var result = dateIn.AddMilliseconds(date.Millisecond);

                // return, because this event handler will be executed a second time
            return result;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(true, null);
            DateTime dateIn;
            return new ValidationResult(DateTime.TryParse((string)value, out dateIn), null);
        }
    }
}
