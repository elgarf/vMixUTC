using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace vMixController.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class ImagePathToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ImagePathToVisibilityConverter());

        private string[] _extensions = { ".bmp", ".gif", ".png", ".ico", ".jpg", ".jpeg", ".tiff", ".dds" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var p = string.IsNullOrEmpty((string)parameter);
            if (value is string v)
            {
                var resolvedPath = Classes.Utils.ResolvePortablePath(v);
                if (_extensions.Contains(Path.GetExtension(v)) && File.Exists(resolvedPath))
                    return p ? Visibility.Visible : Visibility.Collapsed;
            }
            return p ? Visibility.Collapsed : Visibility.Visible;
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


