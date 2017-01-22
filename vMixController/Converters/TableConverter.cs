using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class TableConverter : IMultiValueConverter
    {
        private bool _isList = false;

        public TableConverter(bool isList = false)
        {
            _isList = isList;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string separator = "|";
            if (parameter != null)
                separator = (string)parameter;
            if (values.OfType<string>().Count() > 0)
            {
                if (_isList)
                    return values.OfType<string>().Aggregate((x, y) => Helpers.UnescapeAt((Helpers.EscapeAt(x)).Split(Helpers.EscapeSymbol[0])[0]) + separator + Helpers.UnescapeAt(Helpers.EscapeAt(y).Split(Helpers.EscapeSymbol[0])[0]));
                else
                    return values.OfType<string>().Aggregate((x, y) => x + separator + y);
            }
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
                {
                    if (_isList)
                        values[i] = Helpers.UnescapeAt(Helpers.EscapeAt(separated[i]).Split(Helpers.EscapeSymbol[0])[0]);
                    else
                        values[i] = separated[i];
                }
                else
                    values[i] = "";
            return values;
        }
    }
}
