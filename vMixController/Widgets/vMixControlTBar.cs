using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using vMixController.Classes;
using vMixController.Controls;
using vMixController.PropertiesControls;

namespace vMixController.Widgets
{
    public class vMixControlTBar : vMixControl
    {
        public override string Type => "TBar";
        public override bool IsResizeableVertical => true;
        private bool _reverse = false;
        private bool _reset = false;
        public override int MaxCount => 1;

        public vMixControlTBar()
        {
            Height = 48;
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
        /// The <see cref="Mode" /> property's name.
        /// </summary>
        public const string ModePropertyName = "Mode";

        private string _mode = "A/B";

        /// <summary>
        /// Sets and gets the Mode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                if (_mode == value)
                {
                    return;
                }

                _mode = value;
                RaisePropertyChanged(ModePropertyName);
            }
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(vMixControlTBar), new PropertyMetadata(0, ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = ((vMixControlTBar)d);
            if (e.Property.Name == "Value" && !bar._reset)
            {
                int value = Math.Min((int)e.NewValue, 255);
                bool reverse = bar._reverse;
                value = reverse ? 255 - value : value;
                bar.SendValue(value);
                if (value == 255 && bar.Mode != "Snap Back")
                    ((vMixControlTBar)d)._reverse = !reverse;
            }
            else
                bar._reset = false;
        }

        public override UserControl[] GetPropertiesControls()
        {
            var styleComboBox = GetPropertyControl<ComboBoxControl>();
            styleComboBox.Title = Extensions.LocalizationManager.Get("Style");
            styleComboBox.Items = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "Horizontal",
                "Vertical"
            };
            styleComboBox.Value = Style;

            var modeComboBox = GetPropertyControl<ComboBoxControl>(Type + "Mode");
            modeComboBox.Title = Extensions.LocalizationManager.Get("Mode");
            modeComboBox.Items = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "A/B",
                "Snap Back"
            };
            modeComboBox.Value = Mode;

            return (new UserControl[] { styleComboBox, modeComboBox }).Concat(base.GetPropertiesControls()).ToArray();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            Style = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).FirstOrDefault()).Value;
            Mode = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).LastOrDefault()).Value;

            Value = 0;
            _reverse = false;

            base.SetProperties(_controls);
        }

        public override void Update()
        {
            Height++;
            Height--;

            base.Update();
        }

        private void SendValue(int value)
        {
            if (!_sending)
            {
                if (State != null)
                    _sending = true;
                State?.SendFunction(string.Format("Function=SetFader&Value={0}", value), true, (c) =>
                {
                    _sending = false;
                });
            }
        }

        internal override void OnStateSynced()
        {
            Value = 0;
            _sending = false;
            _reverse = false;
            base.OnStateSynced();
        }

        private RelayCommand<RoutedEventArgs> _valueChangedCommand;
        private bool _sending;

        /// <summary>
        /// Gets the ValueChangedCommand.
        /// </summary>
        public RelayCommand<RoutedEventArgs> ValueChangedCommand
        {
            get
            {
                return _valueChangedCommand
                    ?? (_valueChangedCommand = new RelayCommand<RoutedEventArgs>(
                    (p) =>
                    {
                        if (Value >= 255 && Mode == "Snap Back")
                        {
                            ((TBarSlider)p.Source).CancelDrag();                            
                            _reset = true;
                            Value = 0;
                            ((TBarSlider)p.Source).GetBindingExpression(TBarSlider.ValueProperty).UpdateTarget();
                        }
                    }));
            }
        }
    }
}
