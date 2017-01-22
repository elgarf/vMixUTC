using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class TagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            val = Helpers.EscapeAt(val);
            var ts = val.Split(Helpers.EscapeSymbol[0]);
            if ((string)parameter == "tag")
                if (ts.Length >= 2)
                    val = Helpers.UnescapeAt(ts[1]);
                else
                    val = null;
            else
                val = Helpers.UnescapeAt(ts[0]);



            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
