using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace vMixController
{
    /// <summary>
    /// Description for vMixWidgetSettingsView.
    /// </summary>
    public partial class GlobalVariablesView : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x20000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Initializes a new instance of the vMixWidgetSettingsView class.
        /// </summary>
        public GlobalVariablesView()
        {

            InitializeComponent();
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<bool>(this, x =>
            {
                DialogResult = x;
            });
            Closed += (o, e) => Messenger.Default.Unregister(this);

            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*e.Cancel = true;
            this.Hide();*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            long value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MINIMIZEBOX));
        }
    }
}