using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.Messages;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public partial class vMixControlTimer : vMixControlTextField
    {
        private static readonly Dictionary<string, int> TimerEventIndices = BuildTimerEventIndices();
        bool _changingTime = false;
        private static readonly TimeSpan HighPrecisionInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan NormalInterval = TimeSpan.FromSeconds(1);
        private readonly object _timerSync = new object();

        [NonSerialized]
        private CancellationTokenSource _timerCts;
        [NonSerialized]
        private Task _timerTask;
        [NonSerialized]
        private Stopwatch _timerStopwatch;
        [NonSerialized]
        private TimeSpan _elapsedApplied;
        [NonSerialized]
        private TimeSpan _tickRemainder;
        [NonSerialized]
        private int _pendingHighPrecisionTicks;
        [NonSerialized]
        private int _pendingNormalTicks;
        [NonSerialized]
        private int _pendingFlushScheduled;

        public override string Type
        {
            get
            {
                return "Timer";
            }
        }
        public vMixControlTimer()
        {
            _timerStopwatch = new Stopwatch();

            _width = 256;
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Hotkey[] { new Classes.Hotkey() { Name = "Start" },
            new Classes.Hotkey() { Name = "Pause" },
            new Classes.Hotkey() { Name = "Stop" },
            new Classes.Hotkey() { Name = "+1 Hour" },
            new Classes.Hotkey() { Name = "+1 Minute" },
            new Classes.Hotkey() { Name = "+1 Second" },
            new Classes.Hotkey() { Name = "-1 Hour" },
            new Classes.Hotkey() { Name = "-1 Minute" },
            new Classes.Hotkey() { Name = "-1 Second" }};
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        private void Tick(TimeSpan delta)
        {
            if (!Active) return;

            if (!Reverse)
            {
                var t = Time + delta;
                SendLink(Constants.TIMER_EVENT_ONTICK, t.TotalSeconds / DefaultTime.TotalSeconds);
                if (t < DefaultTime)
                    Time = t;
                else
                {
                    Time = DefaultTime;
                    Finish();
                }
            }
            else
            {
                var t = Time - delta;
                SendLink(Constants.TIMER_EVENT_ONTICK, t.TotalSeconds / DefaultTime.TotalSeconds);
                if (t > TimeSpan.Zero)
                    Time = t;
                else
                {
                    Time = TimeSpan.Zero;
                    Finish();
                }
            }
        }

        private void Finish()
        {
            Paused = false;
            if (Active)
            {
                Active = false;
                StopWorker(resetPhase: true);
            }
            SendLink(Constants.TIMER_EVENT_ONSTOP); // OnStop/OnComplete?
            SendLink(Constants.TIMER_EVENT_ONCOMPLETION);
        }

        private void SendLink(int index, object parameter = null)
        {
            if (Links.Length > index && !string.IsNullOrWhiteSpace(Links[index]))
                Messenger.Default.Send(new HotkeyLinkMessage() { Link = Links[index], Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(parameter) });
        }

        private void SendLink(string name, object parameter = null)
        {
            int index = GetTimerEventIndex(name);
            SendLink(index, parameter);
        }

        private static int GetTimerEventIndex(string name)
        {
            if (string.IsNullOrEmpty(name))
                return -1;
            return TimerEventIndices.TryGetValue(name, out var index) ? index : -1;
        }

        private static Dictionary<string, int> BuildTimerEventIndices()
        {
            var map = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < Constants.TimerEvents.Length; i++)
            {
                map[Constants.TimerEvents[i]] = i;
            }
            return map;
        }

        private bool HasOnTickLink()
        {
            var tickIndex = GetTimerEventIndex(Constants.TIMER_EVENT_ONTICK);
            return tickIndex >= 0 && Links.Length > tickIndex && !string.IsNullOrWhiteSpace(Links[tickIndex]);
        }

        private void UpdateTimer()
        {
            if (!Paused)
                Time = Reverse ? DefaultTime : TimeSpan.Zero;
        }

        private bool _splitText = false;

        /// <summary>
        /// Sets and gets the SplitText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool SplitText
        {
            get => _splitText;
            set => SetPropertyValue(ref _splitText, value, nameof(SplitText));
        }

        private bool _isHighPrecision = false;

        /// <summary>
        /// Sets and gets the IsHighPrecision property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsHighPrecision
        {
            get => _isHighPrecision;
            set
            {
                if (!SetPropertyValue(ref _isHighPrecision, value, nameof(IsHighPrecision)))
                    return;

                lock (_timerSync)
                {
                    var intervalTicks = GetCurrentInterval().Ticks;
                    if (intervalTicks > 0)
                        _tickRemainder = TimeSpan.FromTicks(_tickRemainder.Ticks % intervalTicks);
                }
            }
        }

        private bool _recoverOnSync = true;

        /// <summary>
        /// Sets and gets the IsHighPrecision property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool RecoverOnSync
        {
            get => _recoverOnSync;
            set => SetPropertyValue(ref _recoverOnSync, value, nameof(RecoverOnSync));
        }

        private bool _displayZeroWhenStopped = true;

        /// <summary>
        /// Sets and gets the DisplayZeroWhenStopped property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool DisplayZeroWhenStopped
        {
            get
            {
                return _displayZeroWhenStopped;
            }

            set
            {
                if (_displayZeroWhenStopped == value)
                {
                    return;
                }

                _displayZeroWhenStopped = value;
                RaisePropertyChanged(nameof(DisplayZeroWhenStopped));
            }
        }

        private string _format = @"hh\:mm\:ss";

        /// <summary>
        /// Sets and gets the Format property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Format
        {
            get => _format;
            set => SetPropertyValue(ref _format, value, nameof(Format));
        }


        //START, PAUSE, STOP, COMPLETION, TICK
        private string[] _links = new string[] { "", "", "", "", "" };

        /// <summary>
        /// Sets and gets the Links property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string[] Links
        {
            get => _links;
            set => SetPropertyValue(ref _links, value, nameof(Links));
        }

        private bool _reverse = false;

        /// <summary>
        /// Sets and gets the Reverse property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Reverse
        {
            get => _reverse;
            set => SetPropertyValue(ref _reverse, value, nameof(Reverse), _ => UpdateTimer());
        }

        [NonSerialized]
        private bool _active = false;

        /// <summary>
        /// Sets and gets the Active property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool Active
        {
            get => _active;
            set => SetPropertyValue(ref _active, value, nameof(Active));
        }

        [NonSerialized]
        private bool _paused = false;

        /// <summary>
        /// Sets and gets the Paused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool Paused
        {
            get => _paused;
            set => SetPropertyValue(ref _paused, value, nameof(Paused));
        }

        private TimeSpan _time = TimeSpan.Zero;

        /// <summary>
        /// Sets and gets the Time property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore()]
        public TimeSpan Time
        {
            get
            {
                return _time;
            }

            set
            {
                if (_time == value)
                {
                    return;
                }

                _time = value;

                try
                {
                    _changingTime = true;
                    var txt = _time.ToString(Format);
                    if (_time.TotalMinutes >= 1 && Format.StartsWith("mm"))
                    {
                        var parts = txt.Split(':');
                        parts[0] = (_time.Hours * 60 + _time.Minutes).ToString("00");
                        txt = string.Join(":", parts);
                    }
                    Text = SplitText ? string.Join("|", txt.ToCharArray()) : txt;
                    _changingTime = false;
                }
                catch (Exception)
                {
                    Text = "Wrong Format";
                }

                RaisePropertyChanged(nameof(Time));
            }
        }

        //Timer recovering
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == TextProperty)
            {
                if (!_changingTime && _recoverOnSync)
                {
                    TimeSpan parsed = TimeSpan.Zero;
                    if (TimeSpan.TryParseExact((string)e.NewValue, _format, CultureInfo.InvariantCulture, out parsed)
                        || TimeSpan.TryParseExact((string)e.NewValue, @"mm\:ss", CultureInfo.InvariantCulture, out parsed))
                    //if (TimeSpan.TryParse((string)e.NewValue, out parsed))
                    {
                        _time = parsed;
                        RaisePropertyChanged(nameof(Time));
                        if (_time != TimeSpan.Zero && !Active)
                            Paused = true;
                    }
                }
            }
        }

        private long _timeTicks = 0;

        /// <summary>
        /// Sets and gets the TimeTicks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [Browsable(false)]
        public long TimeTicks
        {
            get => _time.Ticks;
            set => SetPropertyValue(ref _timeTicks, value, nameof(TimeTicks), ticks => Time = new TimeSpan(ticks));
        }

        private TimeSpan _defaultTime = TimeSpan.Zero;

        /// <summary>
        /// Sets and gets the DefaultTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore()]
        public TimeSpan DefaultTime
        {
            get => _defaultTime;
            set => SetPropertyValue(ref _defaultTime, value, nameof(DefaultTime), _ => UpdateTimer());
        }

        private long _defaultTimeTicks = 0;

        /// <summary>
        /// Sets and gets the DefaultTimeTicks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [Browsable(false)]
        public long DefaultTimeTicks
        {
            get => _defaultTime.Ticks;
            set => SetPropertyValue(ref _defaultTimeTicks, value, nameof(DefaultTimeTicks), ticks => DefaultTime = new TimeSpan(ticks));
        }

        public override void ExecuteHotkey(int index)
        {
            TimerCommand.Execute(Hotkey[index].Name);
            //base.ExecuteHotkey(index);
        }

        RelayCommand<string> _timerCommand;

        public RelayCommand<string> TimerCommand
        {
            get => _timerCommand ?? (_timerCommand = new RelayCommand<string>((p) => {
                Timer(p);
            }));
        }
        private void Timer(string p)
        {
            switch (p)
            {
                case "Start":
                    if (!Active)
                    {
                        var wasPaused = Paused;
                        if (!wasPaused) UpdateTimer();
                        Paused = false;
                        Active = true;
                        StartWorker(resetPhase: !wasPaused);
                        SendLink(Constants.TIMER_EVENT_ONSTART);
                    }
                    break;

                case "Pause":
                    if (Active)
                    {
                        Paused = true;
                        Active = false;
                        StopWorker(resetPhase: false);
                        SendLink(Constants.TIMER_EVENT_ONPAUSE);
                    }
                    else if (Paused)
                    {
                        Paused = false;
                        Active = true;
                        StartWorker(resetPhase: false);
                        SendLink(Constants.TIMER_EVENT_ONSTART);
                    }
                    break;

                case "Stop":
                    if (Active)
                    {
                        StopWorker(resetPhase: true);
                        Active = false;
                    }
                    else
                        StopWorker(resetPhase: true);
                    Paused = false;
                    if (DisplayZeroWhenStopped)
                        UpdateTimer();
                    SendLink(Constants.TIMER_EVENT_ONSTOP);
                    break;
                case "+1 Hour":
                    Time = Time.Add(TimeSpan.FromHours(1));
                    break;
                case "-1 Hour":
                    Time = Time.Subtract(TimeSpan.FromHours(1));
                    break;
                case "+1 Minute":
                    Time = Time.Add(TimeSpan.FromMinutes(1));
                    break;
                case "-1 Minute":
                    Time = Time.Subtract(TimeSpan.FromMinutes(1));
                    break;
                case "+1 Second":
                    Time = Time.Add(TimeSpan.FromSeconds(1));
                    break;
                case "-1 Second":
                    Time = Time.Subtract(TimeSpan.FromSeconds(1));
                    break;
            }
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;
            if (managed)
            {
                StopWorker(resetPhase: true);
                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }

        private TimeSpan GetCurrentInterval() => IsHighPrecision ? HighPrecisionInterval : NormalInterval;

        private void StartWorker(bool resetPhase)
        {
            lock (_timerSync)
            {
                if (resetPhase)
                {
                    _tickRemainder = TimeSpan.Zero;
                    _pendingHighPrecisionTicks = 0;
                    _pendingNormalTicks = 0;
                    _pendingFlushScheduled = 0;
                }

                _timerCts?.Cancel();
                _timerCts?.Dispose();
                _timerCts = new CancellationTokenSource();

                _elapsedApplied = TimeSpan.Zero;
                _timerStopwatch = new Stopwatch();
                _timerStopwatch.Restart();
                _timerTask = RunTimerLoopAsync(_timerCts.Token);
            }
        }

        private void StopWorker(bool resetPhase)
        {
            CancellationTokenSource ctsToCancel = null;

            lock (_timerSync)
            {
                ctsToCancel = _timerCts;
                _timerCts = null;
                _timerStopwatch?.Stop();
                _elapsedApplied = TimeSpan.Zero;

                if (resetPhase)
                {
                    _tickRemainder = TimeSpan.Zero;
                    _pendingHighPrecisionTicks = 0;
                    _pendingNormalTicks = 0;
                    _pendingFlushScheduled = 0;
                }
            }

            ctsToCancel?.Cancel();
            ctsToCancel?.Dispose();
        }

        private async Task RunTimerLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int producedTicks = 0;
                bool highPrecision;

                lock (_timerSync)
                {
                    if (!Active)
                        break;

                    highPrecision = IsHighPrecision;
                    var interval = highPrecision ? HighPrecisionInterval : NormalInterval;
                    var elapsed = _timerStopwatch.Elapsed;
                    var delta = elapsed - _elapsedApplied;
                    if (delta < TimeSpan.Zero)
                        delta = TimeSpan.Zero;
                    _elapsedApplied = elapsed;

                    _tickRemainder += delta;
                    if (_tickRemainder >= interval)
                    {
                        producedTicks = (int)(_tickRemainder.Ticks / interval.Ticks);
                        _tickRemainder -= TimeSpan.FromTicks(interval.Ticks * producedTicks);
                    }
                }

                if (producedTicks > 0)
                    EnqueueTicks(highPrecision, producedTicks);

                var sleepMs = highPrecision ? 10 : 25;
                try
                {
                    await Task.Delay(sleepMs, token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void EnqueueTicks(bool highPrecision, int tickCount)
        {
            if (tickCount <= 0)
                return;

            lock (_timerSync)
            {
                if (highPrecision)
                    _pendingHighPrecisionTicks += tickCount;
                else
                    _pendingNormalTicks += tickCount;
            }

            if (Interlocked.Exchange(ref _pendingFlushScheduled, 1) == 0)
                RunOnUiThread(FlushPendingTicks, DispatcherPriority.Send);
        }

        private void FlushPendingTicks()
        {
            var hasOnTickLink = HasOnTickLink();
            while (true)
            {
                int hpTicks;
                int normalTicks;

                lock (_timerSync)
                {
                    hpTicks = _pendingHighPrecisionTicks;
                    normalTicks = _pendingNormalTicks;
                    _pendingHighPrecisionTicks = 0;
                    _pendingNormalTicks = 0;
                }

                if (normalTicks > 0)
                {
                    if (!hasOnTickLink && normalTicks > 1)
                    {
                        Tick(TimeSpan.FromTicks(NormalInterval.Ticks * normalTicks));
                    }
                    else
                    {
                        for (var i = 0; i < normalTicks; i++)
                            Tick(NormalInterval);
                    }
                }

                if (hpTicks > 0)
                {
                    if (!hasOnTickLink && hpTicks > 1)
                    {
                        Tick(TimeSpan.FromTicks(HighPrecisionInterval.Ticks * hpTicks));
                    }
                    else
                    {
                        for (var i = 0; i < hpTicks; i++)
                            Tick(HighPrecisionInterval);
                    }
                }

                Interlocked.Exchange(ref _pendingFlushScheduled, 0);
                lock (_timerSync)
                {
                    if (_pendingHighPrecisionTicks == 0 && _pendingNormalTicks == 0)
                        break;
                }

                if (Interlocked.Exchange(ref _pendingFlushScheduled, 1) != 0)
                    break;
            }
        }

        protected bool SetPropertyValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            onChanged?.Invoke(value);
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}