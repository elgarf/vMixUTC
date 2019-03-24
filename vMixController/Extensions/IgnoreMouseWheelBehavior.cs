using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

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



        /// <summary>
        /// Looks for a child control within a parent by type
        /// </summary>
        private List<T> FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            //T foundChild = null;
            List<T> foundChilds = new List<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T childType))
                    // recursively drill down the tree
                    foundChilds = foundChilds.Union(FindChild<T>(child)).ToList();
                else
                {
                    // child element found.
                    foundChilds.Add((T)child);

                }
            }
            return foundChilds;
        }

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
            var popup = FindChild<Popup>((DependencyObject)sender);
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
