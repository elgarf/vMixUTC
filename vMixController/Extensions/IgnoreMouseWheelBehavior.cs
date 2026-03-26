using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;
using vMixController.Controls;

namespace vMixController.Extensions
{
    public sealed class IgnoreMouseWheelBehavior : Behavior<UIElement>
    {



        public bool IgnoreBehavior
        {
            get { return (bool)GetValue(IgnoreBehaviorProperty); }
            set { SetValue(IgnoreBehaviorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IgnoreBehavior.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IgnoreBehaviorProperty =
            DependencyProperty.Register("IgnoreBehavior", typeof(bool), typeof(IgnoreMouseWheelBehavior), new PropertyMetadata(false));



        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
            base.OnDetaching();
        }

        void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var popup = ((DependencyObject)sender).FindChild<Popup>();
            if (popup.Count(x=>x.IsOpen) == 0 && !IgnoreBehavior)
            {
                e.Handled = true;
                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent
                };
                AssociatedObject.RaiseEvent(e2);
            }
        }
    }
}

