using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vMixController.Converters
{
    public class WidgetsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;
            string page = new string[] { "Main", "Data" } [(int)parameter];
            if (value is ObservableCollection<vMixController.Widgets.vMixControl>)
            {
                foreach (var item in value as ObservableCollection<vMixController.Widgets.vMixControl>)
                {
                    if (item.Page == (int)parameter)
                        count++;
                }
                if (count > 0)
                    return page + " *";
            }

            return page;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
