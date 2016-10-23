using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class FirstValueConverter : IMultiValueConverter
    {
        private object[] _previousValues;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object val = null;
            if (_previousValues == null)
            {
                _previousValues = new object[values.Length];
                val = values.Distinct().FirstOrDefault();
            }
            else
                val = values.Distinct().Except(_previousValues.Distinct()).FirstOrDefault();
            if (val == null)
                val = values.Distinct().FirstOrDefault();
            /*_previousValues = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                _previousValues[i] = val;
            }*/
            values.CopyTo(_previousValues, 0);
            if (!(val is string))
                return null;
            else
                return ((string)val).Split('@')[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] values = new object[targetTypes.Length];
            for (int i = 0; i < targetTypes.Length; i++)
                values[i] = ((string)value).Split('@')[0];
            return values;
        }
    }
}
