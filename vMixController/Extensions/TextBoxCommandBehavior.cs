using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace vMixController.Extensions
{
    public static class TextBoxCommandBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(TextBoxCommandBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static readonly DependencyProperty CommandSourceProperty =
            DependencyProperty.RegisterAttached(
                "CommandSource",
                typeof(object),
                typeof(TextBoxCommandBehavior),
                new PropertyMetadata(null));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);
        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        public static object GetCommandSource(DependencyObject obj) => obj.GetValue(CommandSourceProperty);
        public static void SetCommandSource(DependencyObject obj, object value) => obj.SetValue(CommandSourceProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                textBox.PreviewKeyUp += OnPreviewKeyUp;
                textBox.GotFocus += OnGotFocus;
                textBox.LostFocus += OnLostFocus;
                return;
            }

            textBox.PreviewKeyUp -= OnPreviewKeyUp;
            textBox.GotFocus -= OnGotFocus;
            textBox.LostFocus -= OnLostFocus;
        }

        private static void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                TryExecute(textBox, e, "TextBoxPreviewKeyUpCommand", "HandlePreviewKeyUpCommand");
            }
        }

        private static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                TryExecute(textBox, e, "TextBoxGotFocusCommand", "HandleGotFocusCommand");
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                TryExecute(textBox, e, "TextBoxLostFocusCommand", "HandleLostFocusCommand");
            }
        }

        private static void TryExecute(TextBox textBox, object parameter, params string[] commandPropertyNames)
        {
            var source = GetCommandSource(textBox) ?? textBox.DataContext;
            if (source is null)
            {
                return;
            }

            var type = source.GetType();
            foreach (var propertyName in commandPropertyNames)
            {
                var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if (property?.GetValue(source) is not ICommand command)
                {
                    continue;
                }

                if (!command.CanExecute(parameter))
                {
                    continue;
                }

                command.Execute(parameter);
                return;
            }
        }
    }
}
