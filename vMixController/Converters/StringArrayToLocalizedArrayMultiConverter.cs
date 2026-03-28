using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace vMixController.Converters
{
    public class StringArrayToLocalizedArrayMultiConverter : MarkupExtension, IMultiValueConverter
    {
        private static Dictionary<string, LocalizedItem[]> _localizedCache = new Dictionary<string, LocalizedItem[]>();
        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new StringArrayToLocalizedArrayMultiConverter());
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length < 2 || value[0] == null || value[1] == null)
                return null;

            var cult = (value[1] as CultureInfo).Name;
            if (value[0] is string[] array && parameter is string p)
            {
                var key = $"{p}";
                if (_localizedCache.TryGetValue(key, out LocalizedItem[] result))
                {
                    foreach (var item in result)
                        item.Localized = vMixControllerSkin.Localization.LocalizationManager.Instance[$"{p}.{item.Original}"];
                    return result;
                }
                result = array.Select(x => new LocalizedItem() { Original = x, Localized = vMixControllerSkin.Localization.LocalizationManager.Instance[$"{p}.{x}"] }).ToArray();
                _localizedCache.TryAdd(key, result);
                return result;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            if (value is LocalizedItem[] arr)
                return new object[] { arr.Select(x => x.Original).ToArray(), culture };
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
