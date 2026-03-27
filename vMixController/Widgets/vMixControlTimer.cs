using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HighPrecisionTimer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
    public static class TimerTokens
    {
        public const string HighPrecision = nameof(HighPrecision);
        public const string OneSecond = nameof(OneSecond);
    }

    public static class GlobalTimer
    {
        private static IMessenger _messenger;
        private static IMessenger Messenger
        {
            get
            {
                if (_messenger != null)
                    return _messenger;

                _messenger = AppServices.IsRegistered<IMessenger>()
                    ? AppServices.GetRequiredService<IMessenger>()
                    : WeakReferenceMessenger.Default;
                return _messenger;
            }
        }

        private static int _refCount = 0;
        private static int _highPrecisionRefCount = 0;
        private static bool _isTimerRunning = false;
        private static readonly object _sync = new object();

        private static readonly MultimediaTimer _mtimer = new MultimediaTimer();
        private static readonly Stopwatch _sw = new Stopwatch();
        private static TimeSpan _oneSecondAccum;
        private static TimeSpan _highPrecisionAccum;
        private static bool _isWarmedUp;
        private static readonly TimeSpan HighPrecisionTick = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan NormalTick = TimeSpan.FromMilliseconds(1000);
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        static GlobalTimer()
        {
            _mtimer.Interval = (int)HighPrecisionTick.TotalMilliseconds;
            _mtimer.Resolution = 1;
            _mtimer.Elapsed += OnElapsed;
        }

        public static void WarmUp()
        {
            lock (_sync)
            {
                if (_isWarmedUp)
                    return;
                _isWarmedUp = true;
            }

            // Prime JIT and messenger generic paths before first real timer start.
            Messenger.Send<ValueChangedMessage<TimeSpan>, string>(
                new ValueChangedMessage<TimeSpan>(TimeSpan.Zero),
                TimerTokens.HighPrecision);
            Messenger.Send<ValueChangedMessage<TimeSpan>, string>(
                new ValueChangedMessage<TimeSpan>(TimeSpan.Zero),
                TimerTokens.OneSecond);
        }

        public static void Increment(bool isHighPrecision)
        {
            lock (_sync)
            {
                _refCount++;
                if (isHighPrecision)
                    _highPrecisionRefCount++;

                if (_refCount == 1)
                {
                    _oneSecondAccum = TimeSpan.Zero;
                    _highPrecisionAccum = TimeSpan.Zero;
                    _sw.Restart();
                }

                ReconfigureTimerLocked();
            }
        }

        public static void Decrement(bool isHighPrecision)
        {
            lock (_sync)
            {
                if (_refCount > 0)
                    _refCount--;

                if (isHighPrecision && _highPrecisionRefCount > 0)
                    _highPrecisionRefCount--;

                if (_refCount <= 0)
                {
                    if (_isTimerRunning)
                    {
                        _mtimer.Stop();
                        _isTimerRunning = false;
                    }
                    _sw.Reset();
                    _refCount = 0;
                    _highPrecisionRefCount = 0;
                    _oneSecondAccum = TimeSpan.Zero;
                    _highPrecisionAccum = TimeSpan.Zero;
                    return;
                }

                ReconfigureTimerLocked();
            }
        }

        public static void UpdatePrecisionMode(bool oldIsHighPrecision, bool newIsHighPrecision)
        {
            if (oldIsHighPrecision == newIsHighPrecision)
                return;

            lock (_sync)
            {
                if (_refCount <= 0)
                    return;

                if (oldIsHighPrecision && _highPrecisionRefCount > 0)
                    _highPrecisionRefCount--;
                if (newIsHighPrecision)
                    _highPrecisionRefCount++;

                ReconfigureTimerLocked();
            }
        }

        private static void ReconfigureTimerLocked()
        {
            var tick = _highPrecisionRefCount > 0 ? HighPrecisionTick : NormalTick;
            var newInterval = (int)tick.TotalMilliseconds;
            if (_mtimer.Interval != newInterval)
            {
                if (_isTimerRunning)
                {
                    _mtimer.Stop();
                    _isTimerRunning = false;
                }
                _mtimer.Interval = newInterval;
            }

            if (!_isTimerRunning)
            {
                _mtimer.Start();
                _isTimerRunning = true;
            }
        }

        private static void OnElapsed(object sender, EventArgs e)
        {
            TimeSpan elapsed;
            int oneSecondTicks = 0;
            int highPrecisionTicks = 0;

            // MultimediaTimer callback can re-enter under load; keep timing math serialized.
            lock (_sync)
            {
                if (_refCount <= 0)
                    return;

                elapsed = _sw.Elapsed;
                _sw.Restart();

                _oneSecondAccum += elapsed;
                while (_oneSecondAccum >= OneSecond)
                {
                    oneSecondTicks++;
                    _oneSecondAccum -= OneSecond;
                }

                if (_highPrecisionRefCount > 0)
                {
                    _highPrecisionAccum += elapsed;
                    while (_highPrecisionAccum >= HighPrecisionTick)
                    {
                        highPrecisionTicks++;
                        _highPrecisionAccum -= HighPrecisionTick;
                    }
                }
                else
                {
                    _highPrecisionAccum = TimeSpan.Zero;
                }
            }

            var appDispatcher = Application.Current?.Dispatcher;
            if (appDispatcher == null || appDispatcher.CheckAccess())
            {
                DispatchTick(highPrecisionTicks, oneSecondTicks);
            }
            else
            {
                appDispatcher.BeginInvoke(new Action(() => DispatchTick(highPrecisionTicks, oneSecondTicks)), DispatcherPriority.Send);
            }
        }

        private static void DispatchTick(int highPrecisionTicks, int oneSecondTicks)
        {
            for (var i = 0; i < highPrecisionTicks; i++)
            {
                Messenger.Send<ValueChangedMessage<TimeSpan>, string>(new ValueChangedMessage<TimeSpan>(HighPrecisionTick), TimerTokens.HighPrecision);
            }
            for (var i = 0; i < oneSecondTicks; i++)
            {
                Messenger.Send<ValueChangedMessage<TimeSpan>, string>(new ValueChangedMessage<TimeSpan>(OneSecond), TimerTokens.OneSecond);
            }
        }
    }

    [Serializable]
    public partial class vMixControlTimer : vMixControlTextField
    {
        bool _changingTime = false;
        public override string Type
        {
            get
            {
                return "Timer";
            }
        }
        public vMixControlTimer()
        {
            GlobalTimer.WarmUp();
            Messenger.Register<vMixControlTimer, ValueChangedMessage<TimeSpan>, string>(this, TimerTokens.HighPrecision, (r, m) =>
            {
                if (r.IsHighPrecision) r.RunOnUiThread(() => r.Tick(m.Value), DispatcherPriority.Send);
            });
            Messenger.Register<vMixControlTimer, ValueChangedMessage<TimeSpan>, string>(this, TimerTokens.OneSecond, (r, m) =>
            {
                if (!r.IsHighPrecision) r.RunOnUiThread(() => r.Tick(m.Value), DispatcherPriority.Send);
            });

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
                if (t > TimeSpan.Zero)
                    Time = t;
                else
                {
                    Time = TimeSpan.Zero;
                    Finish();
                }
            }

            if (Links.Length > 4 && !string.IsNullOrWhiteSpace(Links[4]))
                Messenger.Send(new HotkeyLinkMessage() { Link = Links[4], Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(null) });
        }

        private void Finish()
        {
            Paused = false;
            if (Active)
            {
                Active = false;
                GlobalTimer.Decrement(IsHighPrecision);
            }
            SendLink(2); // OnStop/OnComplete?
            SendLink(3);
        }

        private void SendLink(int index)
        {
            if (!string.IsNullOrWhiteSpace(Links[index]))
                Messenger.Send(new HotkeyLinkMessage() { Link = Links[index], Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(null) });
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
                var oldValue = _isHighPrecision;
                if (!SetPropertyValue(ref _isHighPrecision, value, nameof(IsHighPrecision)))
                    return;

                if (Active)
                    GlobalTimer.UpdatePrecisionMode(oldValue, _isHighPrecision);
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
                    if (TimeSpan.TryParseExact((string)e.NewValue, _format, CultureInfo.InvariantCulture, out parsed))
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

        [RelayCommand]
        private void Timer(string p)
        {
            switch (p)
            {
                case "Start":
                    if (!Active)
                    {
                        if (!Paused) UpdateTimer();
                        Paused = false;
                        Active = true;
                        GlobalTimer.Increment(IsHighPrecision);
                        SendLink(0);
                    }
                    break;

                case "Pause":
                    if (Active)
                    {
                        Paused = true;
                        Active = false;
                        GlobalTimer.Decrement(IsHighPrecision);
                        SendLink(1);
                    }
                    else if (Paused)
                    {
                        Paused = false;
                        Active = true;
                        GlobalTimer.Increment(IsHighPrecision);
                        SendLink(0);
                    }
                    break;

                case "Stop":
                    if (Active)
                    {
                        GlobalTimer.Decrement(IsHighPrecision);
                        Active = false;
                    }
                    Paused = false;
                    UpdateTimer();
                    SendLink(2);
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
                Messenger.UnregisterAll(this);
                if (Active) GlobalTimer.Decrement(IsHighPrecision); // а не безусловный --
                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }
}


