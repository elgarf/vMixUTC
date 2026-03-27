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
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToStringConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new EnumToStringConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return Enum.GetName(value.GetType(), value);
            //throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null || !targetType.IsEnum || value == null)
            {
                return Binding.DoNothing;
            }

            var name = value as string;
            if (string.IsNullOrWhiteSpace(name))
            {
                return Binding.DoNothing;
            }

            return Enum.TryParse(targetType, name, out var parsed) ? parsed : Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
