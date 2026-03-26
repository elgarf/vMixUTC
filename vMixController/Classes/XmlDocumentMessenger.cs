using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using vMixAPI;
using vMixController.Classes;
using vMixController.Widgets;

namespace vMixController.Classes
{
    public static class XmlDocumentMessenger
    {
        public delegate void DocumentDownloaded(XmlDocument doc, DateTime timestamp);

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private static readonly object _stateLock = new object();

        private static CancellationTokenSource _cts;
        private static Task _pollLoopTask;

        private static string _url = "http://127.0.0.1:8088/api";
        private static Uri _urlUri = new Uri("http://127.0.0.1:8088/api");
        private static string _credentials;
        private static AuthenticationHeaderValue _authorizationHeader;

        private static int _activeRequests;
        private static int _pollFailures;
        private static long _lastDocumentTimestampTicksUtc;
        private static int _stateRecalcVersion;

        public static bool Sync { get; set; } = false;

        public static string Url
        {
            get { return _url; }
            set
            {
                var normalized = (value ?? "http://127.0.0.1:8088/api").TrimEnd('?');
                _url = normalized;
                if (Uri.TryCreate(normalized, UriKind.Absolute, out var parsed))
                    _urlUri = parsed;
            }
        }

        /// <summary>
        /// ������: "user:password"
        /// </summary>
        public static string Credentials
        {
            get { return _credentials; }
            set
            {
                _credentials = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    _authorizationHeader = null;
                    return;
                }

                var raw = Encoding.UTF8.GetBytes(value);
                _authorizationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(raw));
            }
        }

        public static int Rate { get; set; }

        public static int MaxConcurrentRequests { get; set; } = 1;
        public static long LastDocumentTimestampTicksUtc => Interlocked.Read(ref _lastDocumentTimestampTicksUtc);
        public static int StateRecalcVersion => Volatile.Read(ref _stateRecalcVersion);

        private static event DocumentDownloaded _onDocumentDownloaded;
        public static event DocumentDownloaded OnDocumentDownloaded
        {
            add
            {
                _onDocumentDownloaded += value;
                Debug.WriteLine("XmlDocumentMessenger subscribers: " + (_onDocumentDownloaded == null ? 0 : _onDocumentDownloaded.GetInvocationList().Length));
            }
            remove
            {
                _onDocumentDownloaded -= value;
                Debug.WriteLine("XmlDocumentMessenger subscribers: " + (_onDocumentDownloaded == null ? 0 : _onDocumentDownloaded.GetInvocationList().Length));
            }
        }

        public static event Action<Exception> OnError;

        public static void Start()
        {
            lock (_stateLock)
            {
                if (_pollLoopTask != null && !_pollLoopTask.IsCompleted)
                    return;

                _cts = new CancellationTokenSource();
                _pollLoopTask = Task.Run(() => PollLoopAsync(_cts.Token));
            }
        }

        public static void RequestImmediateUpdate()
        {
            CancellationToken token;
            lock (_stateLock)
            {
                if (_cts == null || _cts.IsCancellationRequested)
                    return;
                token = _cts.Token;
            }

            if (_onDocumentDownloaded == null || !Sync)
                return;

            _ = PollOnceAsync(token);
        }

        public static void RequestImmediateUpdateWithForcedStateRecalc()
        {
            Interlocked.Increment(ref _stateRecalcVersion);
            RequestImmediateUpdate();
        }

        public static async Task StopAsync()
        {
            Task taskToWait = null;

            lock (_stateLock)
            {
                if (_cts == null)
                    return;

                _cts.Cancel();
                taskToWait = _pollLoopTask;
            }

            try
            {
                if (taskToWait != null)
                    await taskToWait.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                lock (_stateLock)
                {
                    if (_cts != null)
                    {
                        _cts.Dispose();
                        _cts = null;
                    }
                    _pollLoopTask = null;
                }
            }
        }

        private static async Task PollLoopAsync(CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();
            TimeSpan nextPollAt = TimeSpan.Zero;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!Sync || _onDocumentDownloaded == null)
                    {
                        await Task.Delay(200, token).ConfigureAwait(false);
                        continue;
                    }

                    TimeSpan pollInterval = GetPollInterval();
                    var failures = Volatile.Read(ref _pollFailures);
                    if (failures > 0)
                    {
                        var multiplier = Math.Min(8, 1 << Math.Min(3, failures));
                        pollInterval = TimeSpan.FromMilliseconds(Math.Min(10000, pollInterval.TotalMilliseconds * multiplier));
                    }

                    if (stopwatch.Elapsed >= nextPollAt)
                    {
                        // fire-and-forget, ���������� MaxConcurrentRequests
                        _ = PollOnceAsync(token);
                        nextPollAt = stopwatch.Elapsed + pollInterval;
                    }

                    TimeSpan delay = nextPollAt - stopwatch.Elapsed;
                    if (delay < TimeSpan.FromMilliseconds(50))
                        delay = TimeSpan.FromMilliseconds(50);

                    await Task.Delay(delay, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    if (token.IsCancellationRequested)
                        break;
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                    await Task.Delay(300, token).ConfigureAwait(false);
                }
            }
        }

        private static async Task PollOnceAsync(CancellationToken token)
        {
            using (PerfMetrics.Measure("poll.once"))
            {
                if (Interlocked.Increment(ref _activeRequests) > MaxConcurrentRequests)
                {
                    Interlocked.Decrement(ref _activeRequests);
                    return;
                }

                try
                {
                    var uri = _urlUri;
                    if (uri == null)
                        return;

                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        if (_authorizationHeader != null)
                            request.Headers.Authorization = _authorizationHeader;

                        using (var response = await _httpClient.SendAsync(request, token).ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();

                            var doc = new XmlDocument();
                            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                doc.Load(stream);
                            }

                            if (!string.Equals(doc.DocumentElement != null ? doc.DocumentElement.Name : null, "vmix", StringComparison.OrdinalIgnoreCase))
                            {
                                Interlocked.Increment(ref _pollFailures);
                                PerfMetrics.Error("poll.invalid-xml");
                                return;
                            }

                            Interlocked.Exchange(ref _pollFailures, 0);
                            RaiseDocumentDownloaded(doc, DateTime.Now);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!token.IsCancellationRequested)
                        RaiseError(new TimeoutException("Request canceled unexpectedly."));
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _pollFailures);
                    PerfMetrics.Error("poll.error");
                    RaiseError(ex);
                }
                finally
                {
                    Interlocked.Decrement(ref _activeRequests);
                }
            }
        }

        private static TimeSpan GetPollInterval()
        {
            double ms = (Rate != 0)
                ? Properties.Settings.Default.AudioMeterPollTime * 1000.0
                : vMixControl.ShadowUpdatePollTime.TotalMilliseconds;

            if (ms < 50) ms = 50;
            return TimeSpan.FromMilliseconds(ms);
        }

        private static void RaiseDocumentDownloaded(XmlDocument doc, DateTime timestamp)
        {
            var handler = _onDocumentDownloaded;
            if (handler == null) return;

            Interlocked.Exchange(ref _lastDocumentTimestampTicksUtc, timestamp.ToUniversalTime().Ticks);

            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => handler(doc, timestamp)));
            }
            else
            {
                handler(doc, timestamp);
            }
        }

        private static void RaiseError(Exception ex)
        {
            Debug.WriteLine("XmlDocumentMessenger error: " + ex);
            var errorHandler = OnError;
            if (errorHandler != null)
                errorHandler(ex);
        }
    }
}

