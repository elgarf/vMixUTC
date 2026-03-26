using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using vMixController.Classes;
using vMixController.Extensions;
using vMixController.ViewModel;

namespace vMixController
{
    /// <summary>
    /// Description for vMixWidgetSettingsView.
    /// </summary>
    public partial class vMixWidgetSettingsView : Window
    {
        private IMessenger Messenger => AppServices.IsRegistered<IMessenger>()
            ? AppServices.GetRequiredService<IMessenger>()
            : WeakReferenceMessenger.Default;

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x20000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        static vMixWidgetSettingsView _instance;
        public static vMixWidgetSettingsView Instance { get {
                return _instance ?? (_instance = new vMixWidgetSettingsView());

            } }

        /// <summary>
        /// Initializes a new instance of the vMixWidgetSettingsView class.
        /// </summary>
        public vMixWidgetSettingsView()
        {
            DataContext = AppServices.GetRequiredService<vMixWidgetSettingsViewModel>();
            InitializeComponent();
            Messenger.Register<vMixWidgetSettingsView, ValueChangedMessage<bool>>(this, (r, m) =>
            {
                r.DialogResult = m.Value;
            });
            Closed += (o, e) => Messenger.UnregisterAll(this);

            
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
            Topmost = Application.Current?.MainWindow?.Topmost ?? false;
            Activate();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {

        }

        private void SaveTemplateClick(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(this) > 0)
            {
                var templateRootElement = VisualTreeHelper.GetChild(this.WidgetProperties, 0);

                // ������ �������� ��� ������, �� � �������� ��������� �����
                // �������� �� ��� ���� (this), � ������ �������� ������� �������.
                templateRootElement?.UpdateAllExplicitSources();
                CommonProperties.UpdateAllExplicitSources();
            }
            
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.SetIsCancelledOnAllChildren();
        }

        public void SaveConnectedWidgetProperties()
        {
            if (VisualTreeHelper.GetChildrenCount(this) > 0)
            {
                var templateRootElement = VisualTreeHelper.GetChild(this.WidgetProperties, 0);

                // ������ �������� ��� ������, �� � �������� ��������� �����
                // �������� �� ��� ���� (this), � ������ �������� ������� �������.
                templateRootElement?.UpdateAllExplicitSources();

                CommonProperties.UpdateAllExplicitSources();

            }
            

        }
    }
}

