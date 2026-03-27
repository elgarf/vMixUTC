
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using vMixController.Classes;
using vMixController.PropertiesControls;

namespace vMixController.Converters
{
    public class ByteToPairBoolStringConverter : MarkupExtension, IMultiValueConverter
    {
        private static IMultiValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IMultiValueConverter Instance => _instance ?? (_instance = new ByteToPairBoolStringConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var vals = new ObservableCollection<Pair<bool, string>>();
            if (values == null || values.Length < 2 || !(values[0] is byte flags) || !(values[1] is Hotkey[] hotkeys))
            {
                return vals;
            }

            var source = hotkeys.Skip(1).ToArray();
            for (byte i = 0; i < 7; i++)
            {
                var name = i < source.Length ? source[i].Name : string.Empty;
                var e = new Pair<bool, string>(flags.GetBit(i), name);
                vals.Add(e);
            }

            return vals;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            byte result = 0;
            if (!(value is ObservableCollection<Pair<bool, string>> vals))
            {
                return new object[] { result, null };
            }

            for (byte i = 0; i < 7; i++)
            {
                if (i < vals.Count)
                {
                    result = result.SetBit(i, vals[i].A);
                }
            }

            return new object[] { result, null };
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
