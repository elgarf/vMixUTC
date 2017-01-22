using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixController.Classes;
using System.Windows.Controls;
using vMixController.PropertiesControls;
using vMixController.ViewModel;
using System.ComponentModel;
using System.Diagnostics;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlTimer : vMixControlTextField
    {
        [NonSerialized]
        Stopwatch _stopwatch = new Stopwatch();
        [NonSerialized]
        DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Send);

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Timer");
            }
        }
        public vMixControlTimer()
        {
            //Text = "00:00:00";
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;

            Width = 256;
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

        public override UserControl[] GetPropertiesControls()
        {

            StringControl control = GetPropertyControl<StringControl>();
            control.Title = "Format";
            control.Value = Format;
            var props = base.GetPropertiesControls();
            props.OfType<BoolControl>().First().Visibility = System.Windows.Visibility.Collapsed;
            return props.Concat(new UserControl[] { control }).ToArray();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!Reverse)
            {
                var t = Time.Add(_stopwatch.Elapsed);
                _stopwatch.Restart();
                if (t <= DefaultTime)
                    Time = t;
                else
                {
                    Time = DefaultTime;
                    Paused = true;
                    Active = false;
                    _timer.Stop();
                }
            }
            else
            {
                var t = Time.Subtract(_stopwatch.Elapsed);
                _stopwatch.Restart();
                if (t > TimeSpan.FromSeconds(0))
                    Time = t;
                else
                {
                    Time = TimeSpan.Zero;
                    Paused = true;
                    Active = false;
                    _timer.Stop();
                }
            }
        }

        private void UpdateTimer()
        {
            //Назад
            if (!Paused)
                if (!Reverse)
                    Time = TimeSpan.Zero;
                else
                    Time = DefaultTime;
        }

        /// <summary>
        /// The <see cref="Format" /> property's name.
        /// </summary>
        public const string FormatPropertyName = "Format";

        private string _format = @"hh\:mm\:ss";

        /// <summary>
        /// Sets and gets the Format property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Format
        {
            get
            {
                return _format;
            }

            set
            {
                if (_format == value)
                {
                    return;
                }

                _format = value;
                RaisePropertyChanged(FormatPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Reverse" /> property's name.
        /// </summary>
        public const string ReversePropertyName = "Reverse";

        private bool _reverse = false;

        /// <summary>
        /// Sets and gets the Reverse property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Reverse
        {
            get
            {
                return _reverse;
            }

            set
            {
                if (_reverse == value)
                {
                    return;
                }

                _reverse = value;

                UpdateTimer();

                RaisePropertyChanged(ReversePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Active" /> property's name.
        /// </summary>
        public const string ActivePropertyName = "Active";
        [NonSerialized]
        private bool _active = false;

        /// <summary>
        /// Sets and gets the Active property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                if (_active == value)
                {
                    return;
                }

                _active = value;
                RaisePropertyChanged(ActivePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Paused" /> property's name.
        /// </summary>
        public const string PausedPropertyName = "Paused";
        [NonSerialized]
        private bool _paused = false;

        /// <summary>
        /// Sets and gets the Paused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool Paused
        {
            get
            {
                return _paused;
            }

            set
            {
                if (_paused == value)
                {
                    return;
                }

                _paused = value;
                RaisePropertyChanged(PausedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Time" /> property's name.
        /// </summary>
        public const string TimePropertyName = "Time";

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
                    var _text = _time.ToString(Format);
                    if (_time.Hours > 0 && Format.StartsWith("mm"))
                    {
                        var t = _text.Split(':');
                        t[0] = string.Format("{0:00}", _time.Hours * 60 + _time.Minutes);
                        _text = t.Aggregate((a, b) => a + ":" + b);
                    }
                    Text = _text;
                }
                catch (Exception)
                {
                    Text = "Wrong Format";
                }

                RaisePropertyChanged(TimePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TimeTicks" /> property's name.
        /// </summary>
        public const string TimeTicksPropertyName = "TimeTicks";

        private long _timeTicks = 0;

        /// <summary>
        /// Sets and gets the TimeTicks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [Browsable(false)]
        public long TimeTicks
        {
            get
            {
                return _time.Ticks;
            }

            set
            {
                if (_timeTicks == value)
                {
                    return;
                }

                _timeTicks = value;
                Time = new TimeSpan(value);
                RaisePropertyChanged(TimeTicksPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DefaultTime" /> property's name.
        /// </summary>
        public const string DefaultTimePropertyName = "DefaultTime";

        private TimeSpan _defaultTime = TimeSpan.Zero;

        /// <summary>
        /// Sets and gets the DefaultTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore()]
        public TimeSpan DefaultTime
        {
            get
            {
                return _defaultTime;
            }

            set
            {
                if (_defaultTime == value)
                {
                    return;
                }

                _defaultTime = value;
                UpdateTimer();
                RaisePropertyChanged(DefaultTimePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DefaultTimeTicks" /> property's name.
        /// </summary>
        public const string DefaultTimeTicksPropertyName = "DefaultTimeTicks";

        private long _defaultTimeTicks = 0;

        /// <summary>
        /// Sets and gets the DefaultTimeTicks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [Browsable(false)]
        public long DefaultTimeTicks
        {
            get
            {
                return _defaultTime.Ticks;
            }

            set
            {
                if (_defaultTimeTicks == value)
                {
                    return;
                }

                _defaultTimeTicks = value;
                DefaultTime = new TimeSpan(value);
                RaisePropertyChanged(DefaultTimeTicksPropertyName);
            }
        }

        public override void ExecuteHotkey(int index)
        {
            TimerCommand.Execute(Hotkey[index].Name);
            base.ExecuteHotkey(index);
        }

        [NonSerialized]
        private RelayCommand<string> _timerCommand;

        /// <summary>
        /// Gets the TimerCommand.
        /// </summary>
        public RelayCommand<string> TimerCommand
        {
            get
            {
                return _timerCommand
                    ?? (_timerCommand = new RelayCommand<string>(
                    p =>
                    {
                        switch (p)
                        {
                            case "Start":
                                Paused = false;
                                Active = true;
                                _stopwatch.Start();
                                _timer.Start();
                                break;
                            case "Pause":

                                if (!Paused)
                                {
                                    if (_timer.IsEnabled && _stopwatch.IsRunning)
                                    {
                                        Paused = true;
                                        Active = false;
                                        if (!Reverse)
                                            Time.Add(_stopwatch.Elapsed);
                                        else
                                            Time.Subtract(_stopwatch.Elapsed);
                                        _stopwatch.Stop();
                                        _timer.Stop();
                                    }

                                }
                                else
                                {
                                    Paused = false;
                                    Active = true;
                                    _stopwatch.Start();
                                    _timer.Start();
                                }
                                break;
                            case "Stop":
                                Active = false;
                                Paused = false;
                                _stopwatch.Stop();
                                _timer.Stop();
                                UpdateTimer();
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
                    }));
            }
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);

        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);
            Format = _controls.OfType<StringControl>().First().Value;
        }

        public override sealed void Dispose()
        {

            _timer.Stop();
            _stopwatch.Stop();
            base.Dispose();
        }

    }
}
