using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace vMixController.Converters
{
    public class ImagePartConverter : MarkupExtension, IMultiValueConverter
    {
        private sealed class CacheEntry
        {
            public long LastWriteTicks { get; set; }
            public ImageSource Image { get; set; }
        }

        private static IMultiValueConverter _instance;
        private static readonly Dictionary<string, CacheEntry> Cache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly object CacheSync = new object();
        private const int MaxCacheEntries = 256;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new ImagePartConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //URI
            //MAX
            //NUMBER

            if (!(values[0] is string && values[1] is int && values[2] is int))
                return null;

            var path = (string)values[0];
            var max = (int)values[1];
            var number = (int)values[2];
            if (max <= 0 || number < 0)
            {
                return null;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var resolvedPath = Classes.Utils.ResolvePortablePath(path);
                    if (!File.Exists(resolvedPath))
                        return null;
                    var writeTicks = File.GetLastWriteTimeUtc(resolvedPath).Ticks;
                    var cacheKey = $"{resolvedPath}|{max}|{number}";
                    lock (CacheSync)
                    {
                        if (Cache.TryGetValue(cacheKey, out var cached) &&
                            cached.LastWriteTicks == writeTicks &&
                            cached.Image != null)
                        {
                            return cached.Image;
                        }
                    }

                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(resolvedPath);

                    bi.EndInit();
                    if (number >= max)
                    {
                        return null;
                    }

                    var partWidth = bi.PixelWidth / max;
                    if (partWidth <= 0)
                    {
                        return null;
                    }

                    var rect = new System.Windows.Int32Rect(partWidth * number, 0, partWidth, bi.PixelHeight);
                    var img = new FormatConvertedBitmap(new CroppedBitmap(bi, rect), PixelFormats.Bgra32, null, 0);
                    if (img.CanFreeze)
                    {
                        img.Freeze();
                    }

                    lock (CacheSync)
                    {
                        Cache[cacheKey] = new CacheEntry
                        {
                            LastWriteTicks = writeTicks,
                            Image = img
                        };

                        if (Cache.Count > MaxCacheEntries)
                        {
                            var first = Cache.Keys.FirstOrDefault();
                            if (!string.IsNullOrEmpty(first))
                            {
                                Cache.Remove(first);
                            }
                        }
                    }

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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}


