using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Threading.Tasks;
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

        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            this.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            base.OnStartup(e);
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception.Source != "mscorlib")
                _logger.Error(e.Exception, "First Chance exception.");
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            foreach (var item in e.Args)
            {
                if (Path.GetExtension(item) == ".vmc")
                {
                    App.Current.Resources["CommandLine"] = item;
                    break;
                }
            }
            
        }
    }
}
