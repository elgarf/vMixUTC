using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Threading.Tasks;
using NCrash.UI;
using NCrash;
using System.Windows.Markup;
using System.IO;

namespace vMixController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static DateTime _compile = new DateTime(2016, 6, 30);
        /*public static Licensing.License License = null;

        static Licensing.License DummyLicense()
        {
            var license = new Licensing.License();
            license.AddFeature("WidgetCount", 5);
            license.AddFeature("ExternalData", false);
            license.AddFeature("Container", false);
            return license;
        }*/

        static App()
        {
            DispatcherHelper.Initialize();

            /*if (DateTime.Now > _compile)
            {
                Ookii.Dialogs.Wpf.TaskDialog td = new Ookii.Dialogs.Wpf.TaskDialog();
                td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                td.WindowTitle = "Trial";
                td.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Information;
                td.Content = "Trial period is end!\nContact elgarf@outlook.com to buy full version or renew trial.";
                td.ShowDialog();
                Environment.Exit(0);
            }*/

            //License = DummyLicense();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var userInterface = new NCrash.WPF.NormalWpfUserInterface() { SendReport = true };
            var sender = new Classes.NCrashLocalSender();
            var settings = new DefaultSettings { Sender = sender, HandleProcessCorruptedStateExceptions = true, UserInterface = userInterface };
            var reporter = new ErrorReporter(settings);

            // Sample NCrash configuration for console applications
            AppDomain.CurrentDomain.UnhandledException += reporter.UnhandledException;
            TaskScheduler.UnobservedTaskException += reporter.UnobservedTaskException;
            App.Current.DispatcherUnhandledException += reporter.DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "Dispatcher unhandled exception.");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "Unobserved task exception.");

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            _logger.Error((Exception)e.ExceptionObject, "Current domain unhandled exception.");
        }
    }
}
