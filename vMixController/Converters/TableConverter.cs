using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class TableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string separator = "|";
            if (parameter != null)
                separator = (string)parameter;
            if (values.OfType<string>().Count() > 0)
                return values.OfType<string>().Aggregate((x, y) => x.Split('@')[0] + separator + y.Split('@')[0]);
            else
                return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] values = new object[targetTypes.Length];
            string separator = "|";
            if (parameter != null)
                separator = (string)parameter;
            var separated = ((string)value).Split(new string[] { separator }, StringSplitOptions.None);
            for (int i = 0; i < targetTypes.Length; i++)
                if (i < separated.Length)
                    values[i] = separated[i].Split('@')[0];
                else
                    values[i] = "";
            return values;
        }
    }
}
