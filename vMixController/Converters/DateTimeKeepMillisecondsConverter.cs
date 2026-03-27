using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class DateTimeKeepMillisecondsConverter : ValidationRule, IValueConverter
    {
        private DateTime date;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                date = dateTime;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return date;

            if (!DateTime.TryParse(value as string ?? value?.ToString(), out DateTime dateIn))
            {
                return date;
            }
            var result = dateIn.AddMilliseconds(date.Millisecond);

                // return, because this event handler will be executed a second time
            return result;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(true, null);
            return new ValidationResult(DateTime.TryParse(value as string ?? value.ToString(), out _), null);
        }
    }
}
