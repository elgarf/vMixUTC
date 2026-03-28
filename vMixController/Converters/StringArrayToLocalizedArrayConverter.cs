using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace vMixController.Converters
{
    public partial class LocalizedItem: ObservableObject
    {
        [ObservableProperty]
        public string _original;
        [ObservableProperty]
        public string _localized;
    }
    public class StringArrayToLocalizedArrayConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new StringArrayToLocalizedArrayConverter());
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string[] array && parameter is string p)
            {
                return array.Select(x=> new LocalizedItem() { Original = x, Localized = vMixControllerSkin.Localization.LocalizationManager.Instance[$"{p}.{x}"] }).ToArray();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LocalizedItem[] arr)
                return arr.Select(x=>x.Original).ToArray();
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
