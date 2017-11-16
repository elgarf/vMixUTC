using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vMixController.Extensions
{
    public class HideCloseButtonOnWindow : System.Windows.Interactivity.Behavior<Window>
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



        #region bunch of native methods

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        protected void Update()
        {
            if (AssociatedObject==null)
                return;
            var hwnd = new System.Windows.Interop.WindowInteropHelper(AssociatedObject).Handle;
            var windowStyle = GetWindowLong(hwnd, GWL_STYLE);
            if (IsEnabled)
                SetWindowLong(hwnd, GWL_STYLE, windowStyle | WS_SYSMENU);
            else
                SetWindowLong(hwnd, GWL_STYLE, windowStyle & ~WS_SYSMENU);
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
    }
}
