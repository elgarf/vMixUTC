using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace vMixController.Converters
{
    [ValueConversion(typeof(string), typeof(Cursor))]
    public class StringToCursorConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new StringToCursorConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return  (Cursor)typeof(Cursors).GetProperty((string)value).GetValue(null);
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
