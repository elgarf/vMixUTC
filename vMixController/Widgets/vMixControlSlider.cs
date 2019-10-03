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
    public class vMixControlSlider : vMixControlTextField
    {
        static string _lastState = null;
        static bool _updating = false;
        static DateTime _prevoiusUpdate = DateTime.Now;
        //[NonSerialized]
        //BackgroundWorker _activeStateUpdateWorker;

        public override bool IsResizeableVertical => true;

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Slider");
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
        /// The <see cref="Target" /> property's name.
        /// </summary>
        public const string TargetPropertyName = "Target";

        private string _target = "Input";

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
                    ((vMixControlSlider)d).UpdateVolume();
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
            }
            State?.SendFunction("Function", func,
                "Value", ((int)(100 * Math.Pow(Value / 100, 1d / 4))).ToString(),
                "Input", InputKey);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(vMixControlSlider), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty F1Property =
            DependencyProperty.Register("F1", typeof(double), typeof(vMixControlSlider), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double F1
        {
            get { return (double)GetValue(F1Property); }
            set { SetValue(F1Property, value); }
        }


        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty F2Property =
            DependencyProperty.Register("F2", typeof(double), typeof(vMixControlSlider), new PropertyMetadata(default(double), InternalSliderPropertyChanged));

        public double F2
        {
            get { return (double)GetValue(F2Property); }
            set { SetValue(F2Property, value); }
        }

        public vMixControlSlider()
        {
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

                BindingOperations.ClearBinding(this, ValueProperty);

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
                "Bus B"
            };
            ctrl1.Value = Target;

            Binding b = new Binding("Value")
            {
                Source = ctrl1,
                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                Converter = new StringBoolConverter(),
                ConverterParameter = "Input"
            };
            BindingOperations.SetBinding(input, UIElement.IsEnabledProperty, b);


            return (new UserControl[] { ctrl1, input, ctrl }).Concat(props).ToArray();
        }

        public override void Update()
        {
            Height++;
            Height--;
            base.Update();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            Target = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).FirstOrDefault()).Value;
            Style = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).LastOrDefault()).Value;
            InputKey = (string)((InputSelectorControl)_controls.Where(x => x is InputSelectorControl).FirstOrDefault()).Value;
            base.SetProperties(_controls);
            UpdateText(null);
        }
    }
}
