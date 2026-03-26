using System;
using System.Buffers.Text;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using vMixController.Classes;
using vMixController.ViewModel;
using vMixControllerSkin.Localization;

namespace vMixController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IServiceProvider Services { get; private set; }

        static DateTime _compile = new DateTime(2016, 6, 30);

        public static byte[] RenderToByteArray(FrameworkElement element, double dpi = 96)
        {
            // Measure and arrange the element
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            // Check for valid dimensions
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;

            if (width <= 0 || height <= 0)
                return null;

            // Create the render target
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                width,
                height,
                dpi,
                dpi,
                PixelFormats.Pbgra32);

            // Render the element
            renderBitmap.Render(element);

            // Encode as PNG (you can change to JpegBitmapEncoder if needed)
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Save to memory stream
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static SplashScreenGdip.SplashScreen SplashScreen;
        static MemoryStream _splashImage = new MemoryStream();
        static App()
        {
            _splashImage = new MemoryStream();
            var buffer = RenderToByteArray(new UTCSplashScreen());
            _splashImage.Write(buffer, 0, buffer.Length);
            var ss = new SplashScreenGdip.SplashScreen(400, _splashImage);
            SplashScreen = ss;
            ss.Closed += Ss_Closed;
            Task.Run(() => ss.Run());

            //var e = (UIElement)Activator.CreateInstance(typeof(UTCSplashScreen));
        }

        private static void Ss_Closed(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                bool activated = false;
                while (!activated)
                {
                    Thread.Sleep(50);
                    
                    activated = Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow.Activate());
                }
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureServices();

            LocalizationManager.Instance.InitializeFromSettings();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            this.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            base.OnStartup(e);

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<vMixWidgetSettingsViewModel>();
            services.AddSingleton<GlobalVariablesViewModel>();
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

            Services = services.BuildServiceProvider();
            AppServices.Configure(Services);
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
