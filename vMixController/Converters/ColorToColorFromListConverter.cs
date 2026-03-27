using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using vMixController.ViewModel;

namespace vMixController.Converters
{
    public class ColorToColorFromListConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ColorToColorFromListConverter());
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = vMixWidgetSettingsViewModel.Colors;
            if (!(value is Color c)) return null;
            return list.Where(x => x.A.A == c.A && x.A.R == c.R && x.A.G == c.G && x.A.B == c.B).FirstOrDefault()?.A;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = vMixWidgetSettingsViewModel.Colors;
            if (!(value is Color c)) return null;
            return list.Where(x => x.A.A == c.A && x.A.R == c.R && x.A.G == c.G && x.A.B == c.B).FirstOrDefault()?.A;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
