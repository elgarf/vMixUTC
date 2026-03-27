using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class FirstValueConverter : IMultiValueConverter
    {
        private bool _isList = false;
        private string _default = null;

        public FirstValueConverter(bool isList = false, string def = null)
        {
            _isList = isList;
            _default = def;
        }

        private object[] _previousValues;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return _default;
            }

            object val;
            if (_previousValues == null || _previousValues.Length != values.Length)
            {
                _previousValues = new object[values.Length];
                val = values[0];
            }
            else
            {
                val = FindFirstNewDistinctValue(values, _previousValues);
            }

            if (val == null)
            {
                val = values[0];
            }

            values.CopyTo(_previousValues, 0);
            if (!(val is string))
                return _default;

            var stringValue = val as string;
            if (_isList)
            {
                return ExtractListValue(stringValue);
            }

            return stringValue ?? _default;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] values = new object[targetTypes.Length];
            var stringValue = value as string;
            for (int i = 0; i < targetTypes.Length; i++)
                if (_isList)
                    values[i] = ExtractListValue(stringValue);
                else
                    values[i] = stringValue ?? _default;
            return values;
        }

        private static object FindFirstNewDistinctValue(object[] current, object[] previous)
        {
            var previousSet = new HashSet<object>(previous);
            var seenCurrent = new HashSet<object>();
            for (int i = 0; i < current.Length; i++)
            {
                var item = current[i];
                if (!seenCurrent.Add(item))
                {
                    continue;
                }

                if (!previousSet.Contains(item))
                {
                    return item;
                }
            }

            return null;
        }

        private static string ExtractListValue(string value)
        {
            var escaped = Helpers.EscapeAt(value);
            if (string.IsNullOrEmpty(escaped))
            {
                return string.Empty;
            }

            var separator = Helpers.EscapeSymbol;
            if (string.IsNullOrEmpty(separator))
            {
                return Helpers.UnescapeAt(escaped);
            }

            var idx = escaped.IndexOf(separator[0]);
            var part = idx >= 0 ? escaped.Substring(0, idx) : escaped;
            return Helpers.UnescapeAt(part);
        }
    }
}
