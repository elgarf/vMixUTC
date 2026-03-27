using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using vMixController.Classes;

namespace vMixController.Converters
{
    public class ColorsListToColorConverter : MarkupExtension, IMultiValueConverter
    {
        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new ColorsListToColorConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return Colors.White;
            }

            if (!(values[0] is List<Triple<Color, Color, string>> source) || !(values[1] is int index))
            {
                return Colors.White;
            }

            var palette = source
                .Select(x => x.A)
                .Where(clr => (0.2126 * (clr.R / 255f) + 0.7152 * (clr.G / 255f) + 0.0722 * (clr.B / 255f)) >= 0.33f)
                .ToArray();

            if (index < 0 || index >= palette.Length)
            {
                return Colors.White;
            }

            return palette[index];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
