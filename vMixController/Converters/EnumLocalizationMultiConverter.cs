using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class EnumLocalizationMultiConverter : IMultiValueConverter
    {
        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new EnumLocalizationMultiConverter());
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Enum enumValue && parameter is string p)
            {
                // Нам не важно значение values[1] (культура), 
                // сам факт его изменения вызовет этот метод.
                string key = $"{p}.{enumValue}";
                return vMixControllerSkin.Localization.LocalizationManager.Instance[key];
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is string val && parameter is string p)
            {
                var key = vMixControllerSkin.Localization.LocalizationManager.Instance.GetKey(val, p);
                if (key != null)
                {
                    var v = key.Substring(p.Length + 1);
                    object[] result = new object[targetTypes.Length];
                    result[0] = Enum.Parse(targetTypes[0], v);
                    result[1] = null;
                    return result;
                }
            }
            return null;
        }
    }
}
