using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace vMixController.Extensions
{
    public class ScrollIntoViewBehavior : Behavior<ListView>
    {
        // DependencyProperty для включения/отключения поведения
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(
                "IsEnabled",
                typeof(bool),
                typeof(ScrollIntoViewBehavior),
                new PropertyMetadata(true));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsEnabled) return;

            if (AssociatedObject.SelectedItem != null)
            {
                AssociatedObject.Dispatcher.Invoke(() =>
                {
                    AssociatedObject.UpdateLayout();
                    AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
                });
            }
        }
    }
}

