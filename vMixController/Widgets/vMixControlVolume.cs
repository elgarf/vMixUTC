using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using vMixController.Classes;
using System.Windows.Controls;
using vMixAPI;
using System.Windows.Data;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Globalization;

namespace vMixController.Widgets
{
    [Serializable]
    public partial class vMixControlVolume : vMixControlTextField
    {
        private static DateTime _previousQuery;
        private static uint _instances = 0;
        private bool _disposing = false;
        //private bool _updating = false;
        private bool _dragging = false;
        [NonSerialized]
        private string _cachedXPathKey;
        [NonSerialized]
        private string _cachedXPath;


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
            if (document == null || _updating) return;
            _updating = true;
            try
            {
                _previousQuery = DateTime.Now;
                var xpath = GetTargetXPath();
                if (string.IsNullOrEmpty(xpath))
                    return;

                var input = document.SelectSingleNode(xpath);
                if (input == null)
                    return;

                var f1 = ParseDouble(input.Attributes["meterF1"]?.Value, 0d);
                var f2 = ParseDouble(input.Attributes["meterF2"]?.Value, 0d);
                var muted = ParseBool(input.Attributes["muted"]?.Value, true);
                var volume = ParseDouble(input.Attributes["volume"]?.Value, 0d);
                var audiobusses = input.Attributes["audiobusses"]?.Value ?? "M";

                Action apply = () =>
                {
                    F1 = Math.Sqrt(Math.Sqrt(f1));
                    F2 = Math.Sqrt(Math.Sqrt(f2));
                    if (!_dragging)
                    {
                        IsMuted = muted;
                        Value = volume;
                        AudioBusses = audiobusses;
                    }
                };

                if (Dispatcher == null || Dispatcher.CheckAccess())
                    apply();
                else
                    Dispatcher.BeginInvoke(apply);
            }
            finally
            {
                _updating = false;
            }
        }

        private string GetTargetXPath()
        {
            var key = string.Concat(Target, "|", InputKey);
            if (string.Equals(_cachedXPathKey, key, StringComparison.Ordinal))
                return _cachedXPath;

            string xpath;
            switch (Target)
            {
                case "Input":
                    if (string.IsNullOrEmpty(InputKey))
                        xpath = null;
                    else
                        xpath = string.Concat("//inputs/input[@key='", InputKey, "']");
                    break;
                case "Master": xpath = "//audio/master"; break;
                case "Bus A": xpath = "//audio/busA"; break;
                case "Bus B": xpath = "//audio/busB"; break;
                case "Bus C": xpath = "//audio/busC"; break;
                case "Bus D": xpath = "//audio/busD"; break;
                case "Bus E": xpath = "//audio/busE"; break;
                case "Bus F": xpath = "//audio/busF"; break;
                case "Bus G": xpath = "//audio/busG"; break;
                default: xpath = null; break;
            }

            _cachedXPathKey = key;
            _cachedXPath = xpath;
            return xpath;
        }

        private static double ParseDouble(string value, double fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : fallback;
        }

        private static bool ParseBool(string value, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            return bool.TryParse(value, out var parsed) ? parsed : fallback;
        }

        [RelayCommand]
        private void UpdateBusses(object p)
        {
            var args = (RoutedEventArgs)p;
            var cb = (CheckBox)args.Source;
            UpdateBus(cb.IsChecked.Value, (string)cb.Tag);
        }

        [RelayCommand]
        private void DragStarted()
        {
            _dragging = true;
        }

        [RelayCommand]
        private void DragCompleted()
        {
            _dragging = false;
        }

        
        
        

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
            XmlDocumentMessenger.OnDocumentDownloaded -= XmlDocumentMessenger_OnDocumentDownloaded;
            XmlDocumentMessenger.Rate--;
            _instances--;
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
                return "Volume";
            }
        }

        private string _style = "Horizontal";//Basic, Basketball, American Football

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Style
        {
            get => _style;
            set => SetPropertyValue(ref _style, value, nameof(Style));
        }

        private bool _showMeters = false;

        /// <summary>
        /// Sets and gets the ShowMeters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowMeters
        {
            get => _showMeters;
            set => SetPropertyValue(ref _showMeters, value, nameof(ShowMeters), isShown =>
            {
                if (!isShown)
                {
                    F1 = 0;
                    F2 = 0;
                }
            });
        }

        private bool _showSlider = true;

        /// <summary>
        /// Sets and gets the ShowSlider property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowSlider
        {
            get => _showSlider;
            set => SetPropertyValue(ref _showSlider, value, nameof(ShowSlider));
        }

        private string _target = "Master";

        /// <summary>
        /// Sets and gets the Target property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Target
        {
            get => _target;
            set => SetPropertyValue(ref _target, value, nameof(Target));
        }

        private string _inputKey = "";

        /// <summary>
        /// Sets and gets the InputKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InputKey
        {
            get => _inputKey;
            set => SetPropertyValue(ref _inputKey, value, nameof(InputKey));
        }

        protected static void InternalSliderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (!((vMixControlTextField)d).IsLive || ((vMixControlVolume)d)._updating)
                return;
            if (e.Property.Name == nameof(Value) && !((vMixControlVolume)d)._disposing)
            {

                ((vMixControlVolume)d).UpdateVolume();
                // ���������� Tuple ��� ���� ��� �������
                var key = Tuple.Create(d, e.Property);

                lock (_pendingUpdates) // ������ �� �������������� ������� � ������� � �������
                {
                    // ��������� ����� ���������� ��������� ��� ���� ���� DO/DP
                    _pendingUpdates[key] = DateTime.Now;
                    // ���� ����� �������� ��� ��� � �������, ��������� ���
                    if (_queuedKeys.Add(key))
                    {
                        _updateQueue.Enqueue(key);
                    }
                }

                /*try
                {
                    ((vMixControlVolume)d).UpdateVolume();
                    var exp = BindingOperations.GetMultiBindingExpression(d, ValueProperty);
                    if (exp != null && exp.Status == BindingStatus.Active)
                    {
                        DelayedUpdate.Enqueue(new Triple<DependencyObject, DependencyProperty, DateTime>() { A = d, B = e.Property, C = DateTime.Now });
                    }
                }
                catch (Exception) { }*/
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
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(vMixControlVolume), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty F1Property =
            DependencyProperty.Register(nameof(F1), typeof(double), typeof(vMixControlVolume), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double F1
        {
            get { return (double)GetValue(F1Property); }
            set { SetValue(F1Property, value); }
        }


        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty F2Property =
            DependencyProperty.Register(nameof(F2), typeof(double), typeof(vMixControlVolume), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

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
            DependencyProperty.Register(nameof(AudioBusses), typeof(string), typeof(vMixControlVolume), new PropertyMetadata("M"));



        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMuted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMutedProperty =
            DependencyProperty.Register(nameof(IsMuted), typeof(bool), typeof(vMixControlVolume), new PropertyMetadata(false));

        public List<Input> Inputs { get => State?.Inputs; }


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

        }


        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void Update()
        {
            Height++;
            Height--;

            base.Update();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
            UpdateText(null);
        }


    }
}


