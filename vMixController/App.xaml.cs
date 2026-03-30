using System;
using System.Buffers.Text;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using System.Linq;

namespace vMixController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string FatalLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fatal.log");
        private static void EmergencyLog(string title, Exception exception = null)
        {
            try
            {
                var text = $"{DateTime.Now:O} {title}{Environment.NewLine}";
                if (exception != null)
                    text += exception + Environment.NewLine;
                File.AppendAllText(FatalLogPath, text);
            }
            catch
            {
                // ignored
            }
        }

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
        private static Window _wpfSplashWindow;
        static MemoryStream _splashImage = new MemoryStream();
        static App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            EnsureDataProvidersNativePath();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                EmergencyLog("Static UnhandledException", e.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (s, e) =>
                EmergencyLog("Static UnobservedTaskException", e.Exception);
        }

        private static void EnsureDataProvidersNativePath()
        {
            try
            {
                var providersDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataProviders");
                if (!Directory.Exists(providersDir))
                    return;

                var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                var parts = currentPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Any(p => string.Equals(p.Trim(), providersDir, StringComparison.OrdinalIgnoreCase)))
                    return;

                Environment.SetEnvironmentVariable("PATH", providersDir + ";" + currentPath);
            }
            catch (Exception ex)
            {
                EmergencyLog("EnsureDataProvidersNativePath failed", ex);
            }
        }

        private static void Ss_Closed(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                for (var attempt = 0; attempt < 100; attempt++)
                {
                    Thread.Sleep(50);

                    try
                    {
                        if (Application.Current == null)
                            return;

                        var mainWindow = Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow);
                        if (mainWindow == null)
                            continue;

                        var activated = Application.Current.Dispatcher.Invoke(() => mainWindow.Activate());
                        if (activated)
                            return;
                    }
                    catch
                    {
                        // ignore activation race during startup
                    }
                }
            });
        }

        private static void TryShowSplashScreen()
        {
            try
            {
                if (_wpfSplashWindow != null)
                    return;

                var splashControl = new UTCSplashScreen();
                _wpfSplashWindow = new Window
                {
                    Content = splashControl,
                    Width = 400,
                    Height = 400,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    AllowsTransparency = false,
                    Background = Brushes.Transparent
                };
                _wpfSplashWindow.Show();
            }
            catch (Exception ex)
            {
                EmergencyLog("TryShowSplashScreen failed", ex);
            }
        }

        public static void CloseSplash()
        {
            try
            {
                SplashScreen?.Close();
            }
            catch (Exception ex)
            {
                EmergencyLog("CloseSplash (gdip) failed", ex);
            }

            try
            {
                if (_wpfSplashWindow != null)
                {
                    _wpfSplashWindow.Close();
                    _wpfSplashWindow = null;
                }
            }
            catch (Exception ex)
            {
                EmergencyLog("CloseSplash (wpf) failed", ex);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureLogging();
            ConfigureServices();

            LocalizationManager.Instance.InitializeFromSettings();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            this.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TryShowSplashScreen();

            base.OnStartup(e);

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private static void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logsDirectory = ResolveWritableLogsDirectory();
            if (logsDirectory == null)
            {
                NLog.LogManager.Configuration = config;
                return;
            }

            var fileTarget = new NLog.Targets.FileTarget("f")
            {
                FileName = Path.Combine(logsDirectory, "${shortdate}.log"),
                Layout = "${longdate} ${uppercase:${level}} ${message} ${exception:format=ToString}"
            };

            var errorTarget = new NLog.Targets.FileTarget("ferr")
            {
                FileName = Path.Combine(logsDirectory, "${shortdate}.log"),
                Layout = "${longdate} STATE: ${event-properties:item=APIReturn}${newline}${longdate} ${uppercase:${level}} ${message} ${exception:format=ToString}"
            };

            config.AddTarget(fileTarget);
            config.AddTarget(errorTarget);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, fileTarget);
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Error, errorTarget);
            NLog.LogManager.Configuration = config;
        }

        private static string ResolveWritableLogsDirectory()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vMix UTC", "logs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vMix UTC", "logs"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(candidate))
                        continue;
                    Directory.CreateDirectory(candidate);
                    return candidate;
                }
                catch (Exception ex)
                {
                    EmergencyLog($"ConfigureLogging candidate failed: {candidate}", ex);
                }
            }

            return null;
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<StateFabriqueAdapter>();
            services.AddSingleton<IStateFactory>(sp => sp.GetRequiredService<StateFabriqueAdapter>());
            services.AddSingleton<IStateSyncService>(sp => sp.GetRequiredService<StateFabriqueAdapter>());
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
            EmergencyLog("DispatcherUnhandledException", e.Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "Unobserved task exception.");
            EmergencyLog("UnobservedTaskException", e.Exception);

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            _logger.Error((Exception)e.ExceptionObject, "Current domain unhandled exception.");
            EmergencyLog("CurrentDomain_UnhandledException", e.ExceptionObject as Exception);
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
