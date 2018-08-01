using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace vMixController
{
    /// <summary>
    /// Description for vMixWidgetSettingsView.
    /// </summary>
    public partial class vMixWidgetSettingsView : Window
    {
        /// <summary>
        /// Initializes a new instance of the vMixWidgetSettingsView class.
        /// </summary>
        public vMixWidgetSettingsView()
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
    }
}