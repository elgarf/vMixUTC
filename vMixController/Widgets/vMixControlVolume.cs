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
using Microsoft.VisualBasic.Devices;
using System.Timers;
using vMixController.ViewModel;
using System.Threading;
using NAudio.CoreAudioApi;
using GalaSoft.MvvmLight.Messaging;
using System.Globalization;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlVolume : vMixControlTextField
    {
        private static DateTime _previousQuery;
        private static uint _instances = 0;
        private bool _disposing = false;


        public vMixControlVolume() : base()
        {
           
            Height = 64;
            
            XmlDocumentMessenger.OnDocumentDownloaded += XmlDocumentMessenger_OnDocumentDownloaded;
            XmlDocumentMessenger.Rate++;
        }

        private void XmlDocumentMessenger_OnDocumentDownloaded(XmlDocument doc, DateTime timestamp)
        {
            UpdateVolumeByXPath(doc);
        }

        private void UpdateVolumeByXPath(XmlDocument document)
        {
            if (document == null) return;
            _previousQuery = DateTime.Now;
            XmlNode input = null;
            switch (Target)
            {
                case "Input": input = document.SelectSingleNode(string.Format("//inputs/input[@key='{0}']", InputKey)); break;
                case "Master": input = document.SelectSingleNode("//audio/master"); break;
                case "Bus A": input = document.SelectSingleNode("//audio/busA"); break;
                case "Bus B": input = document.SelectSingleNode("//audio/busB"); break;
                case "Bus C": input = document.SelectSingleNode("//audio/busC"); break;
                case "Bus D": input = document.SelectSingleNode("//audio/busD"); break;
                case "Bus E": input = document.SelectSingleNode("//audio/busE"); break;
                case "Bus F": input = document.SelectSingleNode("//audio/busF"); break;
                case "Bus G": input = document.SelectSingleNode("//audio/busG"); break;
            }
            if (input == null) return;

            //Update all properties

            var f1 = Convert.ToDouble(input.Attributes["meterF1"]?.Value ?? "0", CultureInfo.InvariantCulture);
            var f2 = Convert.ToDouble(input.Attributes["meterF2"]?.Value ?? "0", CultureInfo.InvariantCulture);
            var muted = Convert.ToBoolean(input.Attributes["muted"]?.Value ?? "True", CultureInfo.InvariantCulture);
            var volume = Convert.ToDouble(input.Attributes["volume"]?.Value ?? "0", CultureInfo.InvariantCulture);
            var audiobusses = input.Attributes["audiobusses"]?.Value ?? "M";

            Dispatcher.Invoke(() =>
            {
                F1 = Math.Pow(f1, 1 / 4d);
                F2 = Math.Pow(f2, 1 / 4d);

                if (!App.Current?.MainWindow?.IsActive ?? false)
                {
                    IsMuted = muted;
                    Value = volume;
                    AudioBusses = audiobusses;
                }
            });
        }

        /*private void _meterTimer_Tick(object sender, EventArgs e)
        {
            if (_instances == 0) return;
            var t = DateTime.Now - _previousQuery;
            if ((t.TotalMilliseconds >= (ShowMeters ? Properties.Settings.Default.AudioMeterPollTime * 1000 : vMixControl.ShadowUpdatePollTime.TotalMilliseconds)) && !_querying)
            {
                _previousQuery = DateTime.Now;
                
                _querying = true;
                WebClient _webClient = new vMixWebClient();
                _webClient.DownloadStringAsync(new Uri((CommonServiceLocator.ServiceLocator.Current.GetInstance<MainViewModel>().Model?.GetUrl() ?? "http://127.0.0.1:8088") + "/api"));
                _webClient.DownloadStringCompleted += Client_DownloadStringCompleted;
            }
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            
            if (e.Error == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(e.Result);
                Messenger.Default.Send(new DocumentMessage() { Document = doc, Type = MessageType.Volume, Timestamp = DateTime.Now });
                _querying = false;
            }
            _previousQuery = DateTime.Now;
            ((WebClient)sender).Dispose();
        }*/

        [NonSerialized]
        private RelayCommand<object> _updateBusses;

        /// <summary>
        /// Gets the UpdateBusses.
        /// </summary>
        public RelayCommand<object> UpdateBusses
        {
            get
            {
                return _updateBusses
                    ?? (_updateBusses = new RelayCommand<object>(
                    p =>
                    {
                        var args = (RoutedEventArgs)p;
                        var cb = ((CheckBox)args.Source);
                        UpdateBus(cb.IsChecked.Value, (string)cb.Tag);
                        return;
                    }));
            }
        }

        protected override void Dispose(bool managed)
        {

            base.Dispose(managed);
            /*if (_meterTimer != null)
                _meterTimer.Tick -= _meterTimer_Tick;
            //if (_meterState != null)
            Messenger.Default.Unregister(this);*/
            XmlDocumentMessenger.OnDocumentDownloaded -= XmlDocumentMessenger_OnDocumentDownloaded;
            XmlDocumentMessenger.Rate--;
            _instances--;
            //_meterState.OnStateCreated -= _meterState_OnStateCreated;

        }

        public override void Dispose()
        {
            
            _disposing = true;
            base.Dispose();
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
            if (e.Property.Name == "Value" && !((vMixControlVolume)d)._disposing)
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

        private void UpdateBus(bool state, string bus)
        {
            var func = "Audio" + (bus == "Mute" ? "" : "Bus") + (state ? "On" : "Off");
            if (bus == "Mute")
            {
                if (Target == "Input")
                    State?.SendFunction("Function", func,
                        "Input", InputKey);
                else if (Target == "Master")
                    State?.SendFunction("Function", Target + func);
                else
                    State?.SendFunction("Function", "BusX" + func,
                        "Value",
                        Target.Replace("Bus ", ""));
                IsMuted = !state;
            }
            else
            {
                State?.SendFunction("Function", func,
                    "Value", bus,
                    "Input", InputKey);

                var busses = new List<string>(AudioBusses.Split(','));
                if (state)
                    busses.Add(bus);
                else
                    busses.Remove(bus);
                if (busses.Count > 0)
                    AudioBusses = busses.Aggregate((x, y) => x + "," + y);
                else
                    AudioBusses = "";

            }
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



        public string AudioBusses
        {
            get { return (string)GetValue(AudioBussesProperty); }
            set { SetValue(AudioBussesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AudioBusses.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AudioBussesProperty =
            DependencyProperty.Register("AudioBusses", typeof(string), typeof(vMixControlVolume), new PropertyMetadata("M"));



        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMuted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMutedProperty =
            DependencyProperty.Register("IsMuted", typeof(bool), typeof(vMixControlVolume), new PropertyMetadata(false));



        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] {
            new Hotkey() { Name = "Toggle Bus M" },
            new Hotkey() { Name = "Toggle Bus A" },
            new Hotkey() { Name = "Toggle Bus B" },
            new Hotkey() { Name = "Toggle Bus C" },
            new Hotkey() { Name = "Toggle Bus D" },
            new Hotkey() { Name = "Toggle Bus E" },
            new Hotkey() { Name = "Toggle Bus F" },
            new Hotkey() { Name = "Toggle Bus G" },
            new Hotkey() { Name = "Toggle Muted" }};
        }

        public override void ExecuteHotkey(int index)
        {
            string[] b = new string[] { "M", "A", "B", "C", "D", "E", "F", "G", "Mute" };
            if (b[index] != "Mute")
                UpdateBus(!AudioBusses.Contains(b[index]), b[index]);
            else
                UpdateBus(IsMuted, b[index]);

        }

        internal override void UpdateText(IList<Pair<string, string>> _paths)
        {

            /*if (!_updating)
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

                        if (input is Input)
                        {
                            Binding b1 = new Binding("Audiobusses")
                            {
                                Source = input,
                                Mode = BindingMode.TwoWay,
                                UpdateSourceTrigger = UpdateSourceTrigger.Default
                            };
                            BindingOperations.SetBinding(this, AudioBussesProperty, b1);
                        }

                        Binding b2 = new Binding("Muted")
                        {
                            Source = input,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.Default
                        };
                        BindingOperations.SetBinding(this, IsMutedProperty, b2);
                    }
                }
                _updating = false;
            }*/
        }


        public override UserControl[] GetPropertiesControls()
        {
            var props = base.GetPropertiesControls();

            foreach (var prop in props)
                prop.Visibility = Visibility.Collapsed;
            //props.OfType<BoolControl>().First().Visibility = System.Windows.Visibility.Collapsed;
            //props.OfType<TitleMappingControl>().First().Visibility = System.Windows.Visibility.Collapsed;


            var input = GetPropertyControl<InputSelectorControl>();
            input.Items = null;
            input.Items = _internalState?.Inputs;
            input.Title = "Input";
            input.Value = InputKey;

            var targetComboBox = GetPropertyControl<ComboBoxControl>(Type + "Target");
            targetComboBox.Title = Extensions.LocalizationManager.Get("Target");
            targetComboBox.Items = new System.Collections.ObjectModel.ObservableCollection<string>
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
            targetComboBox.Value = Target;


            var styleComboBox = GetPropertyControl<ComboBoxControl>(Type + "Style");
            styleComboBox.Title = Extensions.LocalizationManager.Get("Style");
            styleComboBox.Items = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "Horizontal",
                "Vertical"
            };
            styleComboBox.Value = Style;
            styleComboBox.Margin = new Thickness(0, 0, 2, 0);
            Grid.SetColumn(styleComboBox, 0);

            var showMetersBool = GetPropertyControl<BoolControl>(Type + "SM");
            showMetersBool.Title = Extensions.LocalizationManager.Get("Show Meters");
            showMetersBool.Value = ShowMeters;
            showMetersBool.Grouped = true;
            showMetersBool.Margin = new Thickness(2, 0, 2, 0);
            Grid.SetColumn(showMetersBool, 1);

            var showSliderBool = GetPropertyControl<BoolControl>(Type + "SS");
            showSliderBool.Title = Extensions.LocalizationManager.Get("Show Slider");
            showSliderBool.Value = ShowSlider;
            showSliderBool.Grouped = true;
            showSliderBool.Margin = new Thickness(2, 0, 0, 0);
            Grid.SetColumn(showSliderBool, 2);

            var grid = GetPropertyControl<GridControl>(Type + "GR");
            grid.Children.Clear();
            grid.Columns = 3;
            grid.Children.Add(styleComboBox);
            grid.Children.Add(showMetersBool);
            grid.Children.Add(showSliderBool);


            Binding b = new Binding("Value")
            {
                Source = targetComboBox,
                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                Converter = new NKristek.Wpf.Converters.ObjectToStringEqualsParameterToBoolConverter(),//new StringBoolConverter(),
                ConverterParameter = "Input"
            };
            BindingOperations.SetBinding(input, UIElement.IsEnabledProperty, b);


            return (new UserControl[] { targetComboBox, input, /*ctrl, ctrl3, ctrl2*/grid }).Concat(props).ToArray();
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

            var grid = _controls.OfType<GridControl>().FirstOrDefault();

            Target = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).FirstOrDefault()).Value;
            Style = (string)((ComboBoxControl)grid.Children.OfType<ComboBoxControl>().Where(x => x is ComboBoxControl).LastOrDefault()).Value;
            InputKey = (string)((InputSelectorControl)_controls.Where(x => x is InputSelectorControl).FirstOrDefault()).Value;
            ShowMeters = grid.Children.OfType<BoolControl>().Where(x => x is BoolControl && ((BoolControl)x).Title == Extensions.LocalizationManager.Get("Show Meters")).FirstOrDefault().Value;
            ShowSlider = grid.Children.OfType<BoolControl>().Where(x => x is BoolControl && ((BoolControl)x).Title == Extensions.LocalizationManager.Get("Show Slider")).FirstOrDefault().Value;

            base.SetProperties(_controls);
            UpdateText(null);
        }


    }
}
