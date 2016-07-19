using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Licensing.WPF.Extensions
{
    public class MarginSetter
    {
        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }
        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }
        // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty MarginProperty =
               DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter), new UIPropertyMetadata(new Thickness(), MarginChangedCallback));
        public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;
            panel.Loaded += new RoutedEventHandler(panel_Loaded);
        }

        static void panel_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as Panel;
            for (int i = 0; i < panel.Children.Count; i++)
            {
                var child = panel.Children[i];
                var fe = child as FrameworkElement;
                if (fe == null || i == 0) continue;
                fe.Margin = MarginSetter.GetMargin(panel);
            }

        }
    }
}
