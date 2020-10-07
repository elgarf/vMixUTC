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
            //Debug.Print("FVC {0}", (object)values);
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
                return _default;
            else
            {
                if (_isList)
                    return Helpers.UnescapeAt((Helpers.EscapeAt((string)val)).Split(Helpers.EscapeSymbol[0])[0]);
                else
                    return val ?? _default;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //Debug.Print("FVC Back {0}", value);
            object[] values = new object[targetTypes.Length];
            for (int i = 0; i < targetTypes.Length; i++)
                if (_isList)
                    values[i] = Helpers.UnescapeAt((Helpers.EscapeAt((string)value)).Split(Helpers.EscapeSymbol[0])[0]);
                else
                    values[i] = value ?? _default;
            return values;
        }
    }
}
