using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace vMixControllerSkin.Converters
{
    public class StringToGridTable : IValueConverter
    {
        static ResourceDictionary _resources;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (_resources == null)
            {
                _resources = new ResourceDictionary();
                _resources.Source =
                    new Uri("/vMixControllerSkin;component/MainSkin.xaml",
                            UriKind.RelativeOrAbsolute);
            }

            UniformGrid g = new UniformGrid();
            var val = (string)value;
            int col = 0;
            foreach (var txt in val.Split('|'))
            {
                g.Columns++;
                //g.ColumnDefinitions.Add(new ColumnDefinition());
                var t = new TextBlock();
                t.Text = txt;
                t.Style = _resources["CaptionText"] as Style;
                t.TextTrimming = TextTrimming.CharacterEllipsis;
                t.FontWeight = FontWeights.Normal;
                var b = new Border() { Child = t };

                b.BorderThickness = new Thickness(1, 0, 0, 0);
                if (col != 0)
                    b.BorderBrush = Brushes.White;
                Grid.SetColumn(b, col++);
                g.Children.Add(b);
            }
            return g;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
