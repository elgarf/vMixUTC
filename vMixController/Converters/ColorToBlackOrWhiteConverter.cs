using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace vMixController.Converters
{
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    [ValueConversion(typeof(ImageSource), typeof(SolidColorBrush))]
    public class ColorToBlackOrWhiteConverter : MarkupExtension, IValueConverter
    {

        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new ColorToBlackOrWhiteConverter());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color clr = Colors.White;

            if (value == null)
                return Brushes.Black;
            
            if (value is ImageSource)
            {
                var src = (BitmapSource)value;
                int stride = (int)src.PixelWidth * (src.Format.BitsPerPixel / 8);
                byte[] arr = new byte[4];
                src.CopyPixels(new System.Windows.Int32Rect(src.PixelWidth / 2, src.PixelHeight / 2, 1, 1), arr, stride, 0);

                //WriteableBitmap wb = new WriteableBitmap(src);
                //clr = wb.GetPixel(wb.PixelWidth / 2, wb.PixelHeight / 2);
                clr = Color.FromRgb(arr[2], arr[1], arr[0]);
                
            }
            else
                clr = ((Color)value);

            var Y = 0.2126 * (clr.R / 255f) + 0.7152 * (clr.G / 255f) + 0.0722 * (clr.B / 255f);
            if (Y >= 0.5f)
                return Brushes.Black;
            else
                return Brushes.White;
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
