using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using vMixController.ViewModel;

namespace vMixController.Converters
{
    public class CropConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //URI
            //MAX
            //NUMBER

            var path = (string)values[0];
            var max = (int)values[1];
            var number = (int)values[2];
            try
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    var relativePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
                    if (File.Exists(relativePath))
                        bi.UriSource = new Uri(relativePath);
                    else
                    {
                        var cpath = Path.GetDirectoryName(ServiceLocator.Current.GetInstance<MainViewModel>().ControllerPath);
                        var directories = Path.GetDirectoryName(path).Split(Path.DirectorySeparatorChar).Reverse().ToArray();
                        var filename = Path.GetFileName(path);
                        string dir = cpath;
                        int i = 0;
                        while (!File.Exists(Path.Combine(dir, filename)) && i < directories.Length)
                        {
                            dir = Path.Combine(dir, directories[i]);
                            i++;
                        }
                        if (File.Exists(Path.Combine(dir, filename)))
                        {
                            bi.UriSource = new Uri(Path.Combine(dir, filename));
                        }
                        else
                            throw new FileNotFoundException();

                    }    
                    bi.EndInit();
                    var img = new CroppedBitmap(bi, new System.Windows.Int32Rect((bi.PixelWidth / max) * number, 0, bi.PixelWidth / max, bi.PixelHeight));
                    return img;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
