using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Converters;
using vMixController.PropertiesControls;

namespace vMixController.Widgets
{
    [Serializable]
    public partial class vMixControlScore : vMixControlTextField
    {
        public override string Type
        {
            get
            {
                return "Score";
            }
        }

        private string _style = Constants.SCORE_BASIC;//"Basic";//Basic, Basketball, American Football

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Style
        {
            get => _style;
            set => SetPropertyValue(ref _style, value, nameof(Style));
        }

        private byte _enabledButtons = 1;

        /// <summary>
        /// Sets and gets the EnabledButtons property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public byte EnabledButtons
        {
            get => _enabledButtons;
            set => SetPropertyValue(ref _enabledButtons, value, nameof(EnabledButtons));
        }



        /// <summary>
        /// ����������� DependencyProperty ��� FormatString.
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register(
                nameof(FormatString),
                typeof(string),
                typeof(StringControl),
                new PropertyMetadata("0")); // "0" - ��� �������� �� ���������

        /// <summary>
        /// CLR-������� ��� ������� � �������� �� ����.
        /// WPF ���������� GetValue/SetValue �������� ��� ������������������.
        /// </summary>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        static vMixControlScore()
        {
            TextProperty.OverrideMetadata(typeof(vMixControlScore), new System.Windows.PropertyMetadata("", InternalPropertyChanged, CoerceValueCallback));
        }

        public vMixControlScore() :
            base()
        {
            //Text = "0\x200A";
            //_defaultValue = "0\x200A";
            Value = 0;


        }

        private static object CoerceValueCallback(DependencyObject d, object baseValue)
        {
            //Dirty hack for zero =(
            //return (string)baseValue == "0" ? "0\x200A" : baseValue;
            return baseValue;
        }

        internal override IMultiValueConverter ConverterSelector()
        {
            if (!IsTable)
                return new FirstValueConverter(def: "0");
            else
                return new StringsToStringConverter();
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { new Classes.Hotkey() { Name = "Reset" },
            new Classes.Hotkey() { Name = "+1" },
            new Classes.Hotkey() { Name = "+2" },
            new Classes.Hotkey() { Name = "+3" },
            new Classes.Hotkey() { Name = "+4" },
            new Classes.Hotkey() { Name = "+5" },
            new Classes.Hotkey() { Name = "+6" },
            new Classes.Hotkey() { Name = "+10" },
            new Classes.Hotkey() { Name = "-1" }};
    }

        public override void ExecuteHotkey(int index)
        {
            int.TryParse(Text, out int i);
            switch (index)
            {
                case 0:
                    Value = 0;
                    break;
                case 1:
                    Value++;
                    break;
                case 2:
                    Value += 2;
                    break;
                case 3:
                    Value += 3;
                    break;
                case 4:
                    Value += 4;
                    break;
                case 5:
                    Value += 5;
                    break;
                case 6:
                    Value += 6;
                    break;
                case 7:
                    Value += 10;
                    break;
                case 8:
                    Value -= 1;
                    break;
                default:
                    break;
            }
        }

        private int _value = 0;

        /// <summary>
        /// Sets and gets the Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                SetPropertyValue(ref _value, value, nameof(Value), newValue =>
                {
                    Text = newValue.ToString(FormatString);
                });
            }
        }

        [RelayCommand]
        private void AddScore(ControlIntParameter p)
        {
            Value += p.B;
        }

        [RelayCommand]
        private void ResetScore()
        {
            Value = 0;
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {

            base.AfterPropertiesChanged();
        }

        internal override void OnStateSynced()
        {
            int.TryParse(Text, out int val);
            _value = val;
            RaisePropertyChanged(nameof(Value));
            base.OnStateSynced();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}

