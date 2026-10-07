using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace vMixController.Classes
{
    internal sealed class VmixTcpSubscriber : IDisposable
    {
        public event EventHandler ActsReceived;

        private CancellationTokenSource _cts;
        private Task _loopTask;
        private Timer _debounce;
        private const int TcpPort = 8099;
        private const int DebounceMs = 300;

        public void Start(string host)
        {
            Stop();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _loopTask = Task.Run(() => LoopAsync(host, token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _loopTask = null;
            _cts?.Dispose();
            _cts = null;
        }

        private async Task LoopAsync(string host, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        client.ReceiveTimeout = 2000;
                        await client.ConnectAsync(host, TcpPort).ConfigureAwait(false);

                        using (var stream = client.GetStream())
                        using (var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" })
                        using (var reader = new StreamReader(stream))
                        {
                            await writer.WriteLineAsync("SUBSCRIBE ACTS").ConfigureAwait(false);

                            while (!ct.IsCancellationRequested)
                            {
                                string line;
                                try
                                {
                                    line = await reader.ReadLineAsync().ConfigureAwait(false);
                                }
                                catch (IOException) { break; }

                                if (line == null) break;

                                // "SUBSCRIBE OK ACTS" is the subscription confirmation — skip it
                                // actual events arrive as "ACTS OK <function> ..."
                                if (line.StartsWith("ACTS ", StringComparison.Ordinal))
                                    FireDebounced();
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { /* connection failed — retry after delay */ }

                try { await Task.Delay(3000, ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private void FireDebounced()
        {
            _debounce?.Dispose();
            _debounce = new Timer(_ => ActsReceived?.Invoke(this, EventArgs.Empty), null, DebounceMs, Timeout.Infinite);
        }

        public void Dispose()
        {
            Stop();
            _debounce?.Dispose();
        }
    }
}
