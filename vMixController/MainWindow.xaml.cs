using System.Diagnostics;
using System.Windows;
using vMixController.ViewModel;

namespace vMixController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) =>
            {
                Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog();
                dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Yes));
                dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.No));
                dialog.WindowTitle = Extensions.LocalizationManager.Get("Exit confirmation");
                dialog.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning;
                dialog.MainInstruction = Extensions.LocalizationManager.Get("Do you really want to quit?");
                if (dialog.ShowDialog(this).ButtonType == Ookii.Dialogs.Wpf.ButtonType.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    vMixController.Properties.Settings.Default.Save();
                    ViewModelLocator.Cleanup();
                }
            };
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}