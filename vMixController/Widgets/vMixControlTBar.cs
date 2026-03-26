using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using System;
using System.Windows;
using System.Windows.Controls;
using vMixController.Classes;
using vMixController.Controls;

namespace vMixController.Widgets
{
    public partial class vMixControlTBar : vMixControl
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

        private string _style = Classes.Constants.HORIZONTAL;//Basic, Basketball, American Football

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Style
        {
            get => _style;
            set => SetPropertyValue(ref _style, value, nameof(Style));
        }

        private string _mode = Classes.Constants.TBAR_MODE_AB;

        /// <summary>
        /// Sets and gets the Mode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Mode
        {
            get => _mode;
            set => SetPropertyValue(ref _mode, value, nameof(Mode));
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(vMixControlTBar), new PropertyMetadata(0, ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = ((vMixControlTBar)d);
            if (e.Property.Name == nameof(Value) && !bar._reset)
            {
                int value = Math.Min((int)e.NewValue, 255);
                //value = value > 253 ? 255 : value;
                bool reverse = bar._reverse;
                value = reverse ? 255 - value : value;
                bar.SendValue(value);
                if (value == 255 && bar.Mode != Classes.Constants.TBAR_MODE_SNAPBACK)
                    ((vMixControlTBar)d)._reverse = !reverse;
            }
            else
                bar._reset = false;
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            Value = 0;
            _reverse = false;

            base.AfterPropertiesChanged();
        }

        public override void Update()
        {
            Height++;
            Height--;

            base.Update();
        }

        private void SendValue(int value)
        {
            if (!_sending || value == 0 || value == 255)
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

        private bool _sending;

        [RelayCommand]
        private void ValueChanged(RoutedEventArgs p)
        {
            if (Value >= 255 && Mode == Classes.Constants.TBAR_MODE_SNAPBACK)
            {
                ((TBarSlider)p.Source).CancelDrag();
                _reset = true;
                Value = 0;
                ((TBarSlider)p.Source).GetBindingExpression(TBarSlider.ValueProperty).UpdateTarget();
            }
        }
    }
}
