// ��������� �������� ������ �� System.Net.Http
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace FileSystemDataProviderNs
{
    public partial class FileSystemDataProvider : DependencyObject, IvMixDataProviderTextInput, INotifyPropertyChanged
    {
        #region Properties & Commands

        public System.Windows.UIElement CustomUI { get; }
        public bool IsProvidingCustomProperties => false;
        public int Period
        {
            get => _period;
            set
            {
                var normalized = Math.Max(100, value);
                SetPropertyValue(ref _period, normalized, nameof(Period), p =>
                {
                    if (_refreshTimer != null)
                    {
                        _refreshTimer.Interval = TimeSpan.FromMilliseconds(p);
                    }
                });
            }
        }

        private readonly object _valuesSync = new object();
        private string[] _cachedValues = Array.Empty<string>();
        private DateTime _lastRefreshUtc = DateTime.MinValue;
        private readonly DispatcherTimer _refreshTimer;
        private int _period = 1000;

        public string[] Values
        {
            get
            {
                RefreshValuesIfNeeded();
                return _cachedValues;
            }
        }

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(FileSystemDataProvider), new PropertyMetadata(""));

        public ICommand PreviewKeyUp { get; set; }
        public ICommand GotFocus { get; set; }
        public ICommand LostFocus { get; set; }
        [RelayCommand]
        private void HandlePreviewKeyUp(KeyEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (!(p.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && p.Key == Key.Return))
            {
                if (PreviewKeyUp is ICommand command && command.CanExecute(p))
                {
                    command.Execute(p);
                }
            }
        }
        [RelayCommand]
        private void HandlePreviewKeyDown(KeyEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (p.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && p.Key == Key.Return)
            {
                p.Handled = true;
                if (p.Source is TextBox sender)
                {
                    int lastLocation = sender.SelectionStart;
                    sender.Text = sender.Text.Insert(lastLocation, Environment.NewLine);
                    sender.SelectionStart = lastLocation + Environment.NewLine.Length;
                }
            }
            else if (p.Key == Key.Return)
            {
                p.Handled = true;
            }
        }

        [RelayCommand]
        private void HandleGotFocus(RoutedEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (GotFocus != null && GotFocus.CanExecute(p))
            {
                GotFocus.Execute(p);
            }
        }

        [RelayCommand]
        private void HandleLostFocus(RoutedEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (LostFocus != null && LostFocus.CanExecute(p))
            {
                LostFocus.Execute(p);
            }
        }

        private string _path;
        private string _filter = "*.*";
        private bool _includeSub;

        [RelayCommand]
        private void ShowRows()
        {
            new RowsViewer().Bind(this, nameof(Values));
        }

        #endregion

        #region Dependency Properties

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(FileSystemDataProvider), new PropertyMetadata("", OnPropertyChanged));

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FileSystemDataProvider), new PropertyMetadata("*.*", OnPropertyChanged));


        public bool IncludeSubDirectories
        {
            get => (bool)GetValue(IncludeSubDirectoriesProperty);
            set => SetValue(IncludeSubDirectoriesProperty, value);
        }
        public static readonly DependencyProperty IncludeSubDirectoriesProperty =
            DependencyProperty.Register(nameof(IncludeSubDirectories), typeof(bool), typeof(FileSystemDataProvider), new PropertyMetadata(false, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var provider = d as FileSystemDataProvider;
            if (provider == null) return;

            if (e.Property == PathProperty)
                provider._path = (string)e.NewValue;
            else if (e.Property == FilterProperty)
                provider._filter = (string)e.NewValue;
            else
                provider._includeSub = (bool)e.NewValue;

            provider.InvalidateValuesCache();
        }
        #endregion

        #region Interface Implementations & Constructor

        public List<object> GetProperties()
        {
            return new List<object> { Path };
        }

        public void SetProperties(List<object> props)
        {
            if (props == null) return;

            Path = props.ElementAtOrDefault(0) as string;

        }

        public void ShowProperties(Window owner)
        {
            //throw new NotImplementedException();
        }

        public FileSystemDataProvider()
        {
            try
            {
                CustomUI = new OnWidgetUI { DataContext = this };
            }
            catch (Exception e)
            {
                CustomUI = new TextBox { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Period)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void RefreshValuesIfNeeded()
        {
            var refreshInterval = Math.Max(100, Period);
            if ((DateTime.UtcNow - _lastRefreshUtc).TotalMilliseconds < refreshInterval)
            {
                return;
            }

            lock (_valuesSync)
            {
                if ((DateTime.UtcNow - _lastRefreshUtc).TotalMilliseconds < refreshInterval)
                {
                    return;
                }

                try
                {
                    SetError(string.Empty);
                    var newValues = (!string.IsNullOrWhiteSpace(_path) && Directory.Exists(_path))
                        ? Directory.GetFiles(_path, _filter ?? "*.*", _includeSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        : Array.Empty<string>();

                    if (!newValues.SequenceEqual(_cachedValues))
                    {
                        _cachedValues = newValues;
                        RaisePropertyChanged(nameof(Values));
                    }
                }
                catch (Exception ex)
                {
                    SetError($"Error retrieving file list: {ex.Message}");
                    _cachedValues = Array.Empty<string>();
                }
                finally
                {
                    _lastRefreshUtc = DateTime.UtcNow;
                }
            }
        }

        private void InvalidateValuesCache()
        {
            lock (_valuesSync)
            {
                _lastRefreshUtc = DateTime.MinValue;
            }

            RefreshValuesIfNeeded();
            RaisePropertyChanged(nameof(Values));
        }

        private void SetError(string error)
        {
            if (Dispatcher.CheckAccess())
            {
                Error = error;
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => Error = error));
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshValuesIfNeeded();
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            onChanged?.Invoke(value);
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}


