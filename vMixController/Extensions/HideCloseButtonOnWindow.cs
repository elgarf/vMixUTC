using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vMixController.Extensions
{
    static class NativeMethods
    {
        #region bunch of native methods

        public static int GWL_STYLE = -16;
        public static int WS_SYSMENU = 0x80000;

        public static uint MF_BYCOMMAND = 0x00000000;
        public static uint MF_GRAYED = 0x00000001;
        public static uint SC_CLOSE = 0xF060;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion


        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        public static extern IntPtr DestroyMenu(IntPtr hWnd);
    }
    public class HideCloseButtonOnWindow : System.Windows.Interactivity.Behavior<Window>, IDisposable
    {


        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(HideCloseButtonOnWindow), new PropertyMetadata(true, IsEnabledChanged));

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HideCloseButtonOnWindow)d).Update();
            //throw new NotImplementedException();
        }





        IntPtr menuHandle;
        protected void DisableCloseButton(IntPtr _windowHandle)
        {
            if (_windowHandle == null)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            menuHandle = NativeMethods.GetSystemMenu(_windowHandle, false);
            if (menuHandle != IntPtr.Zero)
            {
                NativeMethods.EnableMenuItem(menuHandle, NativeMethods.SC_CLOSE, NativeMethods.MF_BYCOMMAND | NativeMethods.MF_GRAYED);
            }
        }


       // private const uint MF_ENABLED = 0x00000000;

        protected void EnableCloseButton(IntPtr _windowHandle)
        {
            if (_windowHandle == null)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            if (menuHandle != IntPtr.Zero)
            {
                NativeMethods.EnableMenuItem(menuHandle, NativeMethods.SC_CLOSE, NativeMethods.MF_BYCOMMAND | 0);
            }
        }

        protected void Update()
        {
            if (AssociatedObject == null)
                return;
            var hwnd = new System.Windows.Interop.WindowInteropHelper(AssociatedObject).Handle;
            var windowStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE);
            if (IsEnabled)
                EnableCloseButton(hwnd);
            else
                DisableCloseButton(hwnd);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            //throw new NotImplementedException();
        }

        ~HideCloseButtonOnWindow()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool dispose)
        {

        }


        
    }
}
