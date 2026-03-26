using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace vMixController.Classes
{
    public struct StreamDeckEvent
    {
        public vMixStreamDeckLibrary.StreamDeckEvent Type;
        public string Context;
    }
    public class StreamDeckConnector : IDisposable
    {
        public event EventHandler<StreamDeckEvent> OnStreamDeckEvent;
        private bool _disposed = false;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _listenerTask;

        public StreamDeckConnector()
        {
            _listenerTask = Task.Run(async () =>
            {
                while (!_disposed && !_cts.IsCancellationRequested)
                {
                    try
                    {
                        using (var r = new MemoryMessagePipe.MemoryMappedFileMessageReceiver("vMixUTCMMF"))
                        {
                            var msg = r.ReceiveMessage<StreamDeckEvent>(stream =>
                            {
                                var result = new StreamDeckEvent();
                                result.Type = (vMixStreamDeckLibrary.StreamDeckEvent)stream.ReadByte();
                                byte[] buffer = new byte[1024];
                                var len = stream.Read(buffer, 0, 1024);
                                result.Context = Encoding.UTF8.GetString(buffer, 0, len);
                                return result;
                            });

                            if (msg.Context != null && msg.Type != vMixStreamDeckLibrary.StreamDeckEvent.None)
                            {
                                var dispatcher = Application.Current?.Dispatcher;
                                if (dispatcher != null && !dispatcher.CheckAccess())
                                    _ = dispatcher.BeginInvoke(new Action(() => OnStreamDeckEvent?.Invoke(this, msg)));
                                else
                                    OnStreamDeckEvent?.Invoke(this, msg);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        if (_cts.IsCancellationRequested || _disposed)
                            break;
                    }

                    await Task.Delay(100, _cts.Token).ConfigureAwait(false);
                }
            });

        }
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _cts.Cancel();
            try
            {
                _listenerTask?.Wait(500);
            }
            catch (AggregateException)
            {
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}
