using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace vMixController.Extensions
{
    public static class ThumbColorizer
    {
        public static SolidColorBrush GetColor(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(ColorProperty);
        }

        public static void SetColor(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.RegisterAttached(
                "Color", typeof(SolidColorBrush), typeof(ThumbColorizer),
                new UIPropertyMetadata(Brushes.Transparent, OnColorPropertyChanged));

        private static void OnColorPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            Slider slider1 = (Slider)d;
            var cc = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < cc; i++)
            {
                var c = VisualTreeHelper.GetChild(d, i);
            }
            //var thumb = slider.Template.FindName("PART_Thumb", slider) as Thumb;


        }
    }
}
