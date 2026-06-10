using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using vMixAPI;
using vMixControllerDataProvider;

namespace UTCSRTDataProvider
{
    public enum SRTMode
    {
        Listener = 0,
        Caller = 1
    }

    [Description("SRT Monitor")]
    public class SRTDataProvider : IvMixDataProvider, IDisposable, INotifyPropertyChanged
    {
        // Core.Initialize only needs to run once; LibVLC instances are per-provider
        // so each SRT Monitor has fully isolated audio state.
        private static bool _coreInitialized;
        private static readonly object _initLock = new object();
        private static string _vlcDir;

        internal LibVLC LibVLCInstance { get; private set; }

        private static string GetDefaultIp()
        {
            try
            {
                var url = StateFabrique.GetUrl();
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        internal void EnsureVlcInitialized()
        {
            if (LibVLCInstance != null) return;
            lock (_initLock)
            {
                if (!_coreInitialized)
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(typeof(SRTDataProvider).Assembly.Location);
                        _vlcDir = Path.Combine(dir, "libvlc", "win-x64");
                        Core.Initialize(_vlcDir);
                        _coreInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SRTDataProvider] Core.Initialize failed: {ex.Message}");
                        return;
                    }
                }
            }
            try
            {
                LibVLCInstance = new LibVLC(enableDebugLogs: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SRTDataProvider] LibVLC init failed: {ex.Message}");
            }
        }

        private OnWidgetUI _ui;
        private SRTMode _mode = SRTMode.Listener;
        private string _ip = GetDefaultIp();
        private int _port = 4000;
        private int _latency = 200;
        private int _keyLength = 32;
        private string _passphrase = "";
        private string _streamId = "";
        private bool _isAudioEnabled = true;

        public SRTMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value) return;
                _mode = value;
                Notify(nameof(Mode));
                Notify(nameof(IsCallerMode));
                Notify(nameof(ModeIndex));
            }
        }

        public bool IsCallerMode => Mode == SRTMode.Caller;

        public int ModeIndex
        {
            get => (int)Mode;
            set { Mode = (SRTMode)value; }
        }

        public string IP
        {
            get => _ip;
            set { if (_ip == value) return; _ip = value; Notify(nameof(IP)); }
        }

        public int Port
        {
            get => _port;
            set { if (_port == value) return; _port = value; Notify(nameof(Port)); }
        }

        public int Latency
        {
            get => _latency;
            set { if (_latency == value) return; _latency = value; Notify(nameof(Latency)); }
        }

        public int KeyLength
        {
            get => _keyLength;
            set
            {
                if (_keyLength == value) return;
                _keyLength = value;
                Notify(nameof(KeyLength));
                Notify(nameof(KeyLengthIndex));
            }
        }

        public int KeyLengthIndex
        {
            get => _keyLength == 16 ? 0 : _keyLength == 24 ? 1 : 2;
            set { KeyLength = value == 0 ? 16 : value == 1 ? 24 : 32; }
        }

        public string Passphrase
        {
            get => _passphrase;
            set { if (_passphrase == value) return; _passphrase = value; Notify(nameof(Passphrase)); }
        }

        public string StreamId
        {
            get => _streamId;
            set { if (_streamId == value) return; _streamId = value; Notify(nameof(StreamId)); }
        }

        public bool IsAudioEnabled
        {
            get => _isAudioEnabled;
            set { if (_isAudioEnabled == value) return; _isAudioEnabled = value; Notify(nameof(IsAudioEnabled)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public SRTDataProvider()
        {
            EnsureVlcInitialized();
            _ui = new OnWidgetUI { DataContext = this };
        }

        internal string BuildUrl()
        {
            if (_port <= 0 || _port > 65535) return null;
            if (Mode == SRTMode.Caller && string.IsNullOrWhiteSpace(_ip)) return null;

            var sb = new System.Text.StringBuilder("srt://");

            if (Mode == SRTMode.Caller)
                sb.Append(_ip.Trim());

            sb.Append($":{_port}");
            sb.Append($"?mode={Mode.ToString().ToLower()}");
            sb.Append($"&latency={_latency}");

            if (!string.IsNullOrWhiteSpace(_passphrase))
            {
                sb.Append($"&passphrase={Uri.EscapeDataString(_passphrase.Trim())}");
                sb.Append($"&pbkeylen={_keyLength}");
            }

            if (!string.IsNullOrWhiteSpace(_streamId))
                sb.Append($"&streamid={Uri.EscapeDataString(_streamId.Trim())}");

            return sb.ToString();
        }

        public int Period { get; set; }
        public bool IsProvidingCustomProperties => false;
        public UIElement CustomUI => _ui;
        public string[] Values => new string[0];
        public void ShowProperties(Window owner) { }

        public List<object> GetProperties() => new List<object>
        {
            (int)Mode, IP, Port, Latency, KeyLength, Passphrase, StreamId, IsAudioEnabled
        };

        public void SetProperties(List<object> props)
        {
            if (props == null || props.Count == 0) return;
            try
            {
                if (props.Count > 0 && props[0] != null) Mode = (SRTMode)Convert.ToInt32(props[0]);
                if (props.Count > 1) IP = props[1] as string ?? "";
                if (props.Count > 2 && props[2] != null) Port = Convert.ToInt32(props[2]);
                if (props.Count > 3 && props[3] != null) Latency = Convert.ToInt32(props[3]);
                if (props.Count > 4 && props[4] != null) KeyLength = Convert.ToInt32(props[4]);
                if (props.Count > 5) Passphrase = props[5] as string ?? "";
                if (props.Count > 6) StreamId = props[6] as string ?? "";
                if (props.Count > 7 && props[7] != null) IsAudioEnabled = Convert.ToBoolean(props[7]);
            }
            catch { }
        }

        public void Dispose()
        {
            (_ui as IDisposable)?.Dispose();
            LibVLCInstance?.Dispose();
            LibVLCInstance = null;
        }
    }
}
