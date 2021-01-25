using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using System.Xml;
using vMixAPI;
using vMixController.Widgets;

namespace vMixController.Classes
{
    public static class XmlDocumentMessenger
    {
        public static bool Sync { get; set; }
        public static string Url { get => url; set {
                _queries = 0;
                url = value;
            }
        }
        public delegate void DocumentDownloaded(XmlDocument doc, DateTime timestamp);
        static int _subscribers = 0;

        public static int Rate { get; set; }

        //static HttpClient _client;

        static event DocumentDownloaded _onDocumentDownloaded;
        public static event DocumentDownloaded OnDocumentDownloaded
        {
            add
            {
                _onDocumentDownloaded += value;
                _subscribers = _onDocumentDownloaded?.GetInvocationList().Length ?? 0;
                Debug.Print("{0} subscribers", _subscribers);
            }
            remove
            {
                _onDocumentDownloaded -= value;
                _subscribers = _onDocumentDownloaded?.GetInvocationList().Length ?? 0;
                Debug.Print("{0} subscribers", _subscribers);
            }
        }

        static int _queries = 0;
        static DateTime _previousQuery = DateTime.Now;
        static DispatcherTimer _stateDependentTimer;
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger(typeof(XmlDocumentMessenger));
        private static string url;

        static XmlDocumentMessenger()
        {
            _stateDependentTimer = new DispatcherTimer(DispatcherPriority.Render);
            _stateDependentTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.AudioMeterPollTime / 5);
            _stateDependentTimer.Start();
            _stateDependentTimer.Tick += _stateDependentTimer_Tick;

            //_client = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) };
        }

        private static void _stateDependentTimer_Tick(object sender, EventArgs e)
        {
            if (!Sync) return;
            var t = DateTime.Now - _previousQuery;
            if (t.TotalMilliseconds >= (Rate != 0 ? Properties.Settings.Default.AudioMeterPollTime * 1000 : vMixControl.ShadowUpdatePollTime.TotalMilliseconds) && _queries < 5 && _subscribers > 0)
            {
#if DEBUG
                //Debug.WriteLine("{0}, {1}", DateTime.Now, t.TotalMilliseconds);
                //ThreadPool.GetAvailableThreads(out int t1, out int t2);
                //Debug.WriteLine("{0}, {1}", t1, t2);
#endif
                _previousQuery = DateTime.Now;
                _queries++;

                ThreadPool.QueueUserWorkItem(x =>
                {
                    Uri uri = null;
                    if (Uri.TryCreate((Url ?? "http://127.0.0.1:8088") + "/api", UriKind.Absolute, out uri))
                    {
                        try
                        {
                            var client = new vMixWebClient();
                            client.DownloadStringCompleted += Client_DownloadStringCompleted;
                            client.DownloadStringAsync(uri);
                        }
                        catch (Exception)
                        {
                            _queries--;
                        }
                    }
                });


            }
        }

        private static void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(e.Result) && e.Result.StartsWith("<vmix>"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(e.Result);
                        _onDocumentDownloaded?.Invoke(doc, DateTime.Now);
                        _queries--;
                    }
                }
                catch (Exception)
                {
                    _queries--;
                }
            }
            (sender as WebClient).Dispose();
            _previousQuery = DateTime.Now;
        }
    }
}
