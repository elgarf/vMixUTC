using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using vMixController.ViewModel;

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
                var relativePath = Classes.Utils.SearchFile(v, Directory.GetCurrentDirectory());
                string foundPath = Classes.Utils.SearchFile(v, Path.GetDirectoryName(ServiceLocator.Current.GetInstance<MainViewModel>().ControllerPath));

                if (_extensions.Contains(Path.GetExtension(v)) && (File.Exists(v) || File.Exists(relativePath) || File.Exists(foundPath)))
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
