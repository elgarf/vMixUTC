using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class TextIsPathToImageConverter : IValueConverter
    {
        private string[] _extensions = { ".bmp", ".gif", ".png", ".ico", ".jpg", ".jpeg", ".tiff", ".dds" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var p = string.IsNullOrEmpty((string)parameter);
            if (value is string v)
            {
                if (_extensions.Contains(Path.GetExtension(v)))
                    return p ? Visibility.Visible : Visibility.Collapsed;
            }
            return p ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
