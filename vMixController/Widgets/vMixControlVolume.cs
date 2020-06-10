using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using vMixController.Classes;
using System.Windows.Controls;
using vMixController.PropertiesControls;
using vMixController.Extensions;
using vMixAPI;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Threading;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Xml;
using vMixController.Converters;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlVolume : vMixControlTextField
    {
        static string _lastState = null;
        static bool _updating = false;
        static DateTime _prevoiusUpdate = DateTime.Now;
        //[NonSerialized]
        //BackgroundWorker _activeStateUpdateWorker;
        private static DispatcherTimer _meterTimer;
        private static vMixAPI.State _meterState;
        private static DateTime _lastMetering;
        private bool _subscribed = false;

        public vMixControlVolume() : base()
        {
            if (_meterTimer == null)
            {
                _meterTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(0.1) };
                _meterTimer.Start();
            }

            Height = 48;

            _meterTimer.Tick += _meterTimer_Tick;
        }

        private void _meterTimer_Tick(object sender, EventArgs e)
        {
            if (!ShowMeters) return;

            if (this.State == null) return;
            if (_meterState == null || _meterState.GetUrl() != this.State.GetUrl())
            {
                _meterState = new vMixAPI.State();
                _meterState.Configure(this.State.Ip, this.State.Port);
                _meterState.CreateAsync();
            }

            if (!_subscribed)
            {
                _meterState.OnStateCreated += _meterState_OnStateCreated;
                _subscribed = true;
            }

            //if ((DateTime.Now - _lastMetering).TotalSeconds > 0.05)
            _meterState.CreateAsync();
        }

        private void _meterState_OnStateCreated(object sender, EventArgs e)
        {
            _lastMetering = DateTime.Now;
            object input = null;
            switch (Target)
            {
                case "Input": input = GetValueByPath((vMixAPI.State)sender, string.Format("Inputs[{0}]", InputKey)); break;
                case "Master": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[Master]", InputKey)); break;
                case "Bus A": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusA]", InputKey)); break;
                case "Bus B": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusB]", InputKey)); break;
                case "Bus C": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusC]", InputKey)); break;
                case "Bus D": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusD]", InputKey)); break;
                case "Bus E": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusE]", InputKey)); break;
                case "Bus F": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusF]", InputKey)); break;
                case "Bus G": input = GetValueByPath((vMixAPI.State)sender, string.Format("Audio[BusG]", InputKey)); break;
            }
            if (input == null) return;
            var f1 = input.GetType().GetProperty("MeterF1");
            var f2 = input.GetType().GetProperty("MeterF2");
            if (f1 != null && f2 != null)
            {
                F1 = Math.Pow((double)f1.GetValue(input), 1d / 4d);
                F2 = Math.Pow((double)f2.GetValue(input), 1d / 4d);
                //Debug.WriteLine("F1:{0}, F2:{1}", F1, F2);
            }
        }

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
            if (_meterTimer != null)
                _meterTimer.Tick -= _meterTimer_Tick;
            if (_meterState != null)
                _meterState.OnStateCreated -= _meterState_OnStateCreated;

        }

        public override bool IsResizeableVertical => true;

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Volume");
            }
        }

        /// <summary>
        /// The <see cref="Style" /> property's name.
        /// </summary>
        public const string StylePropertyName = "Style";

        private string _style = "Horizontal";//Basic, Basketball, American Football

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Style
        {
            get
            {
                return _style;
            }

            set
            {
                if (_style == value)
                {
                    return;
                }

                _style = value;
                RaisePropertyChanged(StylePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowMeters" /> property's name.
        /// </summary>
        public const string ShowMetersPropertyName = "ShowMeters";

        private bool _showMeters = false;

        /// <summary>
        /// Sets and gets the ShowMeters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowMeters
        {
            get
            {
                return _showMeters;
            }

            set
            {
                if (_showMeters == value)
                {
                    return;
                }

                _showMeters = value;
                if (!_showMeters)
                {
                    F1 = 0;
                    F2 = 0;
                }
                RaisePropertyChanged(ShowMetersPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowSlider" /> property's name.
        /// </summary>
        public const string ShowSliderPropertyName = "ShowSlider";

        private bool _showSlider = true;

        /// <summary>
        /// Sets and gets the ShowSlider property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowSlider
        {
            get
            {
                return _showSlider;
            }

            set
            {
                if (_showSlider == value)
                {
                    return;
                }

                _showSlider = value;
                RaisePropertyChanged(ShowSliderPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Target" /> property's name.
        /// </summary>
        public const string TargetPropertyName = "Target";

        private string _target = "Master";

        /// <summary>
        /// Sets and gets the Target property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Target
        {
            get
            {
                return _target;
            }

            set
            {
                if (_target == value)
                {
                    return;
                }

                _target = value;
                RaisePropertyChanged(TargetPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="InputKey" /> property's name.
        /// </summary>
        public const string InputKeyPropertyName = "InputKey";

        private string _inputKey = "";

        /// <summary>
        /// Sets and gets the InputKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InputKey
        {
            get
            {
                return _inputKey;
            }

            set
            {
                if (_inputKey == value)
                {
                    return;
                }

                _inputKey = value;
                RaisePropertyChanged(InputKeyPropertyName);
            }
        }

        protected static void InternalSliderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!((vMixControlTextField)d).IsLive)
                return;
            if (e.Property.Name == "Value")
            {
                try
                {
                    ((vMixControlVolume)d).UpdateVolume();
                    var exp = BindingOperations.GetMultiBindingExpression(d, ValueProperty);
                    if (exp != null && exp.Status == BindingStatus.Active)
                    {
                        DelayedUpdate.Enqueue(new Triple<DependencyObject, DependencyProperty, DateTime>() { A = d, B = e.Property, C = DateTime.Now });
                    }
                }
                catch (Exception) { }
            }
        }

        private void UpdateVolume()
        {
            var func = "SetVolume";
            switch (Target)
            {
                case "Headphones": func = "SetHeadphonesVolume"; break;
                case "Master": func = "SetMasterVolume"; break;
                case "Bus A": func = "SetBusAVolume"; break;
                case "Bus B": func = "SetBusBVolume"; break;
                case "Bus C": func = "SetBusCVolume"; break;
                case "Bus D": func = "SetBusDVolume"; break;
                case "Bus E": func = "SetBusEVolume"; break;
                case "Bus F": func = "SetBusFVolume"; break;
                case "Bus G": func = "SetBusGVolume"; break;
            }
            State?.SendFunction("Function", func,
                "Value", ((int)(100 * Math.Pow(Value / 100, 1d / 4))).ToString(),
                "Input", InputKey);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(vMixControlVolume), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty F1Property =
            DependencyProperty.Register("F1", typeof(double), typeof(vMixControlVolume), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double F1
        {
            get { return (double)GetValue(F1Property); }
            set { SetValue(F1Property, value); }
        }


        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty F2Property =
            DependencyProperty.Register("F2", typeof(double), typeof(vMixControlVolume), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double F2
        {
            get { return (double)GetValue(F2Property); }
            set { SetValue(F2Property, value); }
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { };
        }

        public override void ExecuteHotkey(int index)
        {

        }

        internal override void UpdateText(IList<Pair<string, string>> _paths)
        {

            if (!_updating)
            {
                _updating = true;

                //BindingOperations.ClearBinding(this, ValueProperty);

                //update text
                if (State != null)
                //foreach (var item in _paths)
                {
                    object input = null;
                    switch (Target)
                    {
                        case "Input": input = GetValueByPath(State, string.Format("Inputs[{0}]", InputKey)); break;
                        case "Master": input = GetValueByPath(State, string.Format("Audio[Master]", InputKey)); break;
                        case "Bus A": input = GetValueByPath(State, string.Format("Audio[BusA]", InputKey)); break;
                        case "Bus B": input = GetValueByPath(State, string.Format("Audio[BusB]", InputKey)); break;
                        case "Bus C": input = GetValueByPath(State, string.Format("Audio[BusC]", InputKey)); break;
                        case "Bus D": input = GetValueByPath(State, string.Format("Audio[BusD]", InputKey)); break;
                        case "Bus E": input = GetValueByPath(State, string.Format("Audio[BusE]", InputKey)); break;
                        case "Bus F": input = GetValueByPath(State, string.Format("Audio[BusF]", InputKey)); break;
                        case "Bus G": input = GetValueByPath(State, string.Format("Audio[BusG]", InputKey)); break;
                    }
                    if (input != null)
                    {
                        Binding b = new Binding("Volume")
                        {
                            Source = input,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.Default
                        };
                        BindingOperations.SetBinding(this, ValueProperty, b);
                    }
                }
                _updating = false;
            }
        }


        public override UserControl[] GetPropertiesControls()
        {
            var props = base.GetPropertiesControls();

            props.OfType<BoolControl>().First().Visibility = System.Windows.Visibility.Collapsed;
            props.OfType<TitleMappingControl>().First().Visibility = System.Windows.Visibility.Collapsed;

            var input = GetPropertyControl<InputSelectorControl>();
            input.Items = null;
            input.Items = _internalState?.Inputs;
            input.Title = "Input";
            input.Value = InputKey;

            var ctrl = GetPropertyControl<ComboBoxControl>();
            ctrl.Title = Extensions.LocalizationManager.Get("Style");
            ctrl.Items = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "Horizontal",
                "Vertical"
            };
            ctrl.Value = Style;

            var ctrl1 = GetPropertyControl<ComboBoxControl>();
            ctrl1.Title = Extensions.LocalizationManager.Get("Target");
            ctrl1.Items = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "Input",
                "Master",
                "Bus A",
                "Bus B",
                "Bus C",
                "Bus D",
                "Bus E",
                "Bus F",
                "Bus G"
            };
            ctrl1.Value = Target;

            var ctrl2 = new BoolControl();
            ctrl2.Title = Extensions.LocalizationManager.Get("Show Meters");
            ctrl2.Value = ShowMeters;

            var ctrl3 = new BoolControl();
            ctrl3.Title = Extensions.LocalizationManager.Get("Show Slider");
            ctrl3.Value = ShowSlider;

            Binding b = new Binding("Value")
            {
                Source = ctrl1,
                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                Converter = new StringBoolConverter(),
                ConverterParameter = "Input"
            };
            BindingOperations.SetBinding(input, UIElement.IsEnabledProperty, b);


            return (new UserControl[] { ctrl1, input, ctrl, ctrl3, ctrl2 }).Concat(props).ToArray();
        }

        public override void Update()
        {
            Height++;
            Height--;
            base.Update();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            foreach (var item in _controls)
                item.Visibility = Visibility.Visible;
            Target = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).FirstOrDefault()).Value;
            Style = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).LastOrDefault()).Value;
            InputKey = (string)((InputSelectorControl)_controls.Where(x => x is InputSelectorControl).FirstOrDefault()).Value;
            ShowMeters = (bool)((BoolControl)_controls.Where(x => x is BoolControl && ((BoolControl)x).Title == Extensions.LocalizationManager.Get("Show Meters")).FirstOrDefault()).Value;
            ShowSlider = (bool)((BoolControl)_controls.Where(x => x is BoolControl && ((BoolControl)x).Title == Extensions.LocalizationManager.Get("Show Slider")).FirstOrDefault()).Value;
            base.SetProperties(_controls);
            UpdateText(null);
        }
    }
}
