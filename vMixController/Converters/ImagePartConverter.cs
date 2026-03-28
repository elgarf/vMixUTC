using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            public long LastAccessTicks { get; set; }
            public ImageSource Image { get; set; }
        }

        private static IMultiValueConverter _instance;
        private static readonly Dictionary<string, CacheEntry> Cache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly object CacheSync = new object();
        private const int MaxCacheEntries = 256;
        private static readonly TimeSpan CacheEntryTtl = TimeSpan.FromMinutes(10);

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
                            cached.LastAccessTicks = DateTime.UtcNow.Ticks;
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
                        var nowTicks = DateTime.UtcNow.Ticks;
                        Cache[cacheKey] = new CacheEntry
                        {
                            LastWriteTicks = writeTicks,
                            LastAccessTicks = nowTicks,
                            Image = img
                        };

                        TrimCacheLocked(nowTicks);
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

        private static void TrimCacheLocked(long nowTicks)
        {
            if (Cache.Count == 0)
                return;

            var ttlBoundary = nowTicks - CacheEntryTtl.Ticks;

            if (Cache.Count > MaxCacheEntries / 2)
            {
                var expiredKeys = Cache
                    .Where(p => p.Value == null || p.Value.Image == null || p.Value.LastAccessTicks < ttlBoundary)
                    .Select(p => p.Key)
                    .ToArray();
                foreach (var key in expiredKeys)
                    Cache.Remove(key);
            }

            if (Cache.Count <= MaxCacheEntries)
                return;

            var toRemoveCount = Cache.Count - MaxCacheEntries;
            var lruKeys = Cache
                .OrderBy(p => p.Value?.LastAccessTicks ?? long.MinValue)
                .Take(toRemoveCount)
                .Select(p => p.Key)
                .ToArray();

            foreach (var key in lruKeys)
                Cache.Remove(key);
        }
    }
}


