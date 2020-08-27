using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using vMixController.Classes;

namespace vMixController.Converters
{
    [ValueConversion(typeof(ICollection<One<string>>), typeof(string))]
    public class ParametersToStringConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ParametersToStringConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (ICollection<One<string>>)value;
            if (val.Count > 0)
                return val.Skip(1).Select(x => x.A).Aggregate((x, y) => x + y);
            return "";
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
