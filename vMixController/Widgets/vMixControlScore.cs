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
using System.Windows.Data;
using vMixController.Converters;
using System.Windows;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlScore : vMixControlTextField
    {

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Score");
            }
        }

        /// <summary>
        /// The <see cref="Style" /> property's name.
        /// </summary>
        public const string StylePropertyName = "Style";

        private string _style = "Basic";//Basic, Basketball, American Football

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

        static vMixControlScore()
        {
            TextProperty.OverrideMetadata(typeof(vMixControlScore), new System.Windows.PropertyMetadata("", InternalPropertyChanged, CoerceValueCallback));
        }

        public vMixControlScore():
            base()
        {
            Text = "0";
            _defaultValue = "0";
            
            
        }

        private static object CoerceValueCallback(DependencyObject d, object baseValue)
        {
            //Dirty hack for zero =(
            return (string)baseValue == "0" ? "0\x200A" : baseValue;
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
            new Classes.Hotkey() { Name = "+5" },
            new Classes.Hotkey() { Name = "+6" }};
        }

        public override void ExecuteHotkey(int index)
        {
            int.TryParse(Text, out int i);
            switch (index)
            {
                case 0:
                    Text = "0";
                    break;
                case 1:
                    Text = (i + 1).ToString();
                    break;
                case 2:
                    Text = (i + 2).ToString();
                    break;
                case 3:
                    Text = (i + 3).ToString();
                    break;
                case 4:
                    Text = (i + 5).ToString();
                    break;
                case 5:
                    Text = (i + 6).ToString();
                    break;
                default:
                    break;
            }

            //base.ExecuteHotkey(index);
        }

        [NonSerialized]
        private RelayCommand<ControlIntParameter> _addScoreCommand;

        /// <summary>
        /// Gets the AddScoreCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand<ControlIntParameter> AddScoreCommand
        {
            get
            {
                return _addScoreCommand
                    ?? (_addScoreCommand = new RelayCommand<ControlIntParameter>(
                    p =>
                    {
                        int.TryParse(((vMixControlTextField)p.A).Text, out int _out);
                        _out += p.B;
                        ((vMixControlTextField)p.A).Text = _out.ToString();
                    }));
            }
        }

        [NonSerialized]
        private RelayCommand _resetScoreCommand;

        /// <summary>
        /// Gets the ResetScoreCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand ResetScoreCommand
        {
            get
            {
                return _resetScoreCommand
                    ?? (_resetScoreCommand = new RelayCommand(
                    () =>
                    {
                        Text = "0";
                    }));
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            var props = base.GetPropertiesControls();
            props.OfType<BoolControl>().First().Visibility = System.Windows.Visibility.Collapsed;

            var styleComboBox = GetPropertyControl<ComboBoxControl>(Type + "SCS");
            styleComboBox.Title = Extensions.LocalizationManager.Get("Style");
            styleComboBox.Items = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "Basic",
                "Basketball",
                "Rugby",
                "American Football"
            };
            styleComboBox.Value = Style;

            return (new UserControl[] { styleComboBox }).Concat(props).ToArray();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            Style = (string)((ComboBoxControl)_controls.Where(x => x is ComboBoxControl).FirstOrDefault()).Value;
            base.SetProperties(_controls);
        }
    }
}
