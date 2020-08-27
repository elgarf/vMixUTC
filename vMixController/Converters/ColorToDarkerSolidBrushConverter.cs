using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using vMixController.Classes;

namespace vMixController.Converters
{
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class ColorToDarkerSolidBrushConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ColorToDarkerSolidBrushConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Black;
            var c = (Color)value;
            var hsb = SimpleColorTransforms.RgBtoHsb(c);
            return new SolidColorBrush(SimpleColorTransforms.HsBtoRgb(hsb[0], Math.Min(1, hsb[1] * 1.5), hsb[2] * 0.75));
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
