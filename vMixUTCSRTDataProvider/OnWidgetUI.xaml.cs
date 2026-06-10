using LibVLCSharp.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace UTCSRTDataProvider
{
    public partial class OnWidgetUI : UserControl, IDisposable
    {
        private static int _nextId = 0;
        private readonly int _id = Interlocked.Increment(ref _nextId);

        private VlcMediaPlayer _mediaPlayer;
        private LibVLC _vlc;
        private string _lastUrl;
        private bool _isConnected;
        private bool _reconnecting;
        private bool _disposed;

        static OnWidgetUI()
        {
            Trace.Listeners.Remove("Default");
        }

        public OnWidgetUI()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            VideoPlaceholder.Text = $"Aguardando... [#{_id}]";
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is SRTDataProvider neu)
                AudioCheckBox.IsChecked = neu.IsAudioEnabled;
        }

        private SRTDataProvider Provider => DataContext as SRTDataProvider;

        private void AudioCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Provider != null) Provider.IsAudioEnabled = true;
            if (_isConnected) DoReconnect();
        }

        private void AudioCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Provider != null) Provider.IsAudioEnabled = false;
            if (_isConnected) DoReconnect();
        }

        private Media BuildMedia()
        {
            var media = new Media(_vlc, _lastUrl, FromType.FromLocation);
            media.AddOption(":network-caching=300");
            if (AudioCheckBox.IsChecked != true)
                media.AddOption(":no-audio");
            return media;
        }

        // Reconnect without explicit Stop — Play(newMedia) makes VLC transition internally.
        // _reconnecting suppresses the intermediate Stopped event so video stays visible.
        private void DoReconnect()
        {
            if (_mediaPlayer == null || _vlc == null || string.IsNullOrEmpty(_lastUrl)) return;
            _reconnecting = true;
            SetStatus("Reconectando...", null);
            var media = BuildMedia();
            _mediaPlayer.Play(media);
            media.Dispose();
        }

        private void ShowVideo()
        {
            VideoView.Visibility = Visibility.Visible;
            VideoPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void HideVideo()
        {
            VideoView.Visibility = Visibility.Collapsed;
            VideoPlaceholder.Visibility = Visibility.Visible;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            _vlc = Provider?.LibVLCInstance;
            if (_vlc == null)
            {
                SetStatus("VLC não encontrado na pasta 'libvlc/win-x64'", false);
                return;
            }

            _lastUrl = Provider?.BuildUrl();
            if (string.IsNullOrEmpty(_lastUrl))
            {
                SetStatus(Provider?.Mode == SRTMode.Caller
                    ? "Preencha IP e Porta"
                    : "Preencha a Porta", false);
                return;
            }

            if (_mediaPlayer == null)
            {
                _mediaPlayer = new VlcMediaPlayer(_vlc);
                _mediaPlayer.Playing += VlcMediaPlayer_Playing;
                _mediaPlayer.EncounteredError += VlcMediaPlayer_Error;
                _mediaPlayer.Stopped += VlcMediaPlayer_Stopped;
                _mediaPlayer.EndReached += VlcMediaPlayer_Stopped;
            }

            ShowVideo();
            VideoView.MediaPlayer = _mediaPlayer;
            _mediaPlayer.Stop();
            SetStatus("Conectando...", null);

            var media = BuildMedia();
            _mediaPlayer.Play(media);
            media.Dispose();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _reconnecting = false;
            _isConnected = false;
            _mediaPlayer?.Stop();
            SetStatus("Desconectado", false);
            HideVideo();
        }

        private void VlcMediaPlayer_Playing(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _reconnecting = false;
                _isConnected = true;
                SetStatus("Conectado", true);
            }));
        }

        private void VlcMediaPlayer_Error(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _reconnecting = false;
                _isConnected = false;
                SetStatus("Erro de conexão", false);
                HideVideo();
            }));
        }

        private void VlcMediaPlayer_Stopped(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_reconnecting) return;
                if (StatusText.Text == "Conectado")
                {
                    _isConnected = false;
                    SetStatus("Desconectado", false);
                    HideVideo();
                }
            }));
        }

        private void SetStatus(string text, bool? connected)
        {
            StatusText.Text = text;
            var color = connected == true
                ? Colors.LimeGreen
                : connected == false
                    ? Colors.OrangeRed
                    : Colors.Orange;
            var brush = new SolidColorBrush(color);
            StatusText.Foreground = brush;
            StatusIndicator.Fill = brush;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            DataContextChanged -= OnDataContextChanged;

            if (_mediaPlayer != null)
            {
                _mediaPlayer.Playing -= VlcMediaPlayer_Playing;
                _mediaPlayer.EncounteredError -= VlcMediaPlayer_Error;
                _mediaPlayer.Stopped -= VlcMediaPlayer_Stopped;
                _mediaPlayer.EndReached -= VlcMediaPlayer_Stopped;
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }
        }
    }
}
