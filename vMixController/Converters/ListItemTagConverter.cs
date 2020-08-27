using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace vMixController.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ListItemTagConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ListItemTagConverter());

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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
