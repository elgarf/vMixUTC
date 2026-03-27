using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace vMixControllerDataProvider
{
    public abstract class PollingTextInputDataProviderBase : DependencyObject, IvMixDataProviderTextInput, INotifyPropertyChanged, IDisposable
    {
        private readonly DispatcherTimer _refreshTimer;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private int _refreshScheduled;
        private bool _disposed;
        private int _period = 1000;
        private string[] _valuesCache = Array.Empty<string>();

        protected PollingTextInputDataProviderBase()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Period)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Period
        {
            get => _period;
            set
            {
                var normalized = Math.Max(MinPeriodMs, value);
                SetPropertyValue(ref _period, normalized, nameof(Period), p =>
                {
                    _refreshTimer.Interval = TimeSpan.FromMilliseconds(p);
                    OnPeriodChanged(p);
                });
            }
        }

        public virtual bool IsProvidingCustomProperties => false;

        public virtual string[] Values => _valuesCache;

        public virtual UIElement CustomUI { get; protected set; }

        public ICommand PreviewKeyUp { get; set; }
        public ICommand GotFocus { get; set; }
        public ICommand LostFocus { get; set; }

        public abstract List<object> GetProperties();
        public abstract void SetProperties(List<object> props);
        public abstract void ShowProperties(Window owner);

        protected virtual int MinPeriodMs => 250;

        protected bool IsDisposed => _disposed;

        protected void StartPolling(bool runImmediately = true)
        {
            _refreshTimer.Start();
            if (runImmediately)
            {
                ScheduleRefresh();
            }
        }

        protected void StopPolling()
        {
            _refreshTimer.Stop();
        }

        protected void ScheduleRefresh()
        {
            if (_disposed)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _refreshScheduled, 1, 0) != 0)
            {
                return;
            }

            _ = RefreshCoreAsync();
        }

        protected void SetValuesCache(string[] newValues, bool skipIfEqual = true)
        {
            var normalized = newValues ?? Array.Empty<string>();
            RunOnUi(() =>
            {
                if (skipIfEqual && _valuesCache.SequenceEqual(normalized))
                {
                    return;
                }

                _valuesCache = normalized;
                RaisePropertyChanged(nameof(Values));
            });
        }

        protected void RunOnUi(Action action, DispatcherPriority priority = DispatcherPriority.Background)
        {
            if (action == null || _disposed)
            {
                return;
            }

            if (Application.Current?.Dispatcher?.CheckAccess() ?? true)
            {
                action();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(action, priority);
        }

        protected bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged = null)
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

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() ?? true)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))));
            }
        }

        protected virtual void OnPeriodChanged(int newPeriodMs)
        {
        }

        protected abstract Task RefreshValuesAsync();

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            ScheduleRefresh();
        }

        private async Task RefreshCoreAsync()
        {
            var lockTaken = false;
            try
            {
                await _asyncLock.WaitAsync();
                lockTaken = true;
                if (_disposed)
                {
                    return;
                }

                await RefreshValuesAsync();
            }
            finally
            {
                if (lockTaken)
                {
                    _asyncLock.Release();
                }

                Interlocked.Exchange(ref _refreshScheduled, 0);
            }
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _refreshTimer.Stop();
            _refreshTimer.Tick -= RefreshTimer_Tick;
            _asyncLock.Dispose();
        }
    }
}
