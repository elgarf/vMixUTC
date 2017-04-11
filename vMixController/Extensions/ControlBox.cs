using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace vMixController.Extensions
{
    public static class ControlBox
    {
        public static readonly DependencyProperty HasHelpButtonProperty = DependencyProperty.RegisterAttached(
            "HasHelpButton",
            typeof(bool),
            typeof(ControlBox),
            new UIPropertyMetadata(false, OnControlBoxChanged));

        public static readonly DependencyProperty HasMaximizeButtonProperty = DependencyProperty.RegisterAttached(
            "HasMaximizeButton",
            typeof(bool),
            typeof(ControlBox),
            new UIPropertyMetadata(true, OnControlBoxChanged));

        public static readonly DependencyProperty HasMinimizeButtonProperty = DependencyProperty.RegisterAttached(
            "HasMinimizeButton",
            typeof(bool),
            typeof(ControlBox),
            new UIPropertyMetadata(true, OnControlBoxChanged));

        private const int Style = -16;
        private const int ExtStyle = -20;

        private const int MaximizeBox = 0x10000;
        private const int MinimizeBox = 0x20000;
        private const int ContextHelp = 0x400;

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetHasHelpButton(Window element)
        {
            return (bool)element.GetValue(HasHelpButtonProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetHasHelpButton(Window element, bool value)
        {
            element.SetValue(HasHelpButtonProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetHasMaximizeButton(Window element)
        {
            return (bool)element.GetValue(HasMaximizeButtonProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetHasMaximizeButton(Window element, bool value)
        {
            element.SetValue(HasMaximizeButtonProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetHasMinimizeButton(Window element)
        {
            return (bool)element.GetValue(HasMinimizeButtonProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetHasMinimizeButton(Window element, bool value)
        {
            element.SetValue(HasMinimizeButtonProperty, value);
        }

        private static void OnControlBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
            {
                var hWnd = new WindowInteropHelper(window).Handle;
                if (hWnd == IntPtr.Zero)
                {
                    window.SourceInitialized += OnWindowSourceInitialized;
                }
                else
                {
                    UpdateStyle(window, hWnd);
                    UpdateExtendedStyle(window, hWnd);
                }
            }
        }

        private static void OnWindowSourceInitialized(object sender, EventArgs e)
        {
            var window = (Window)sender;

            var hWnd = new WindowInteropHelper(window).Handle;
            UpdateStyle(window, hWnd);
            UpdateExtendedStyle(window, hWnd);

            window.SourceInitialized -= OnWindowSourceInitialized;
        }

        private static void UpdateStyle(Window window, IntPtr hWnd)
        {
            var style = NativeMethods.GetWindowLong(hWnd, Style);

            if (GetHasMaximizeButton(window))
            {
                style |= MaximizeBox;
            }
            else
            {
                style &= ~MaximizeBox;
            }

            if (GetHasMinimizeButton(window))
            {
                style |= MinimizeBox;
            }
            else
            {
                style &= ~MinimizeBox;
            }

            NativeMethods.SetWindowLong(hWnd, Style, style);
        }

        private static void UpdateExtendedStyle(Window window, IntPtr hWnd)
        {
            var style = NativeMethods.GetWindowLong(hWnd, ExtStyle);

            if (GetHasHelpButton(window))
            {
                style |= ContextHelp;
            }
            else
            {
                style &= -~ContextHelp;
            }

            NativeMethods.SetWindowLong(hWnd, ExtStyle, style);
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            internal static extern int GetWindowLong(IntPtr hWnd, int index);

            [DllImport("user32.dll")]
            internal static extern int SetWindowLong(IntPtr hWnd, int index, int newLong);
        }
    }
}
